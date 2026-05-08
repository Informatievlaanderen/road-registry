namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenDryRunMigratingRoadNetwork;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateDryRunRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public class ForDebugging
{
    private readonly ITestOutputHelper _outputHelper;
    private Mock<ITicketing> TicketingMock { get; } = new();

    public ForDebugging(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    //[Fact]
    [Fact(Skip = "For debugging purposes only")]
    public async Task WithCustomRequestOnActualServer()
    {
        // Arrange
        var zipPath = @"01b55c6f420c4e97b67cf8ca79c53a19\01b55c6f420c4e97b67cf8ca79c53a19.zip";
        var nisCode = "44021_095500-197000_500x500";
        var downloadId = DownloadId.Parse("01b55c6f420c4e97b67cf8ca79c53a19");

        var sp = BuildServiceProvider();

        await using var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
        var inwinningsZone = new Inwinningszone
        {
            DownloadId = downloadId,
            Completed = false,
            Contour = Polygon.Empty,
            NisCode = nisCode,
            Operator = "0425258688"
        };
        extractsDbContext.Inwinningszones.Add(inwinningsZone);
        var provenanceData = new RoadRegistryProvenanceData(operatorName: inwinningsZone.Operator);

        TranslatedChanges translatedChanges;
        using (var fileStream = File.OpenRead(zipPath))
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient(nameof(GrbOgcApiFeaturesDownloader));
            var baseUrl = $"https://geo.api.vlaanderen.be/GRB/ogc/features/v1";

            var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(
                grbOgcApiFeaturesDownloader: new GrbOgcApiFeaturesDownloader(client, baseUrl));
            var archive = new ZipArchive(fileStream);

            var zipArchiveMetadata = ZipArchiveMetadata.Empty.WithInwinning();
            translatedChanges = await translator.TranslateAsync(archive, zipArchiveMetadata, CancellationToken.None);

            var extractRoadSegments = new RoadSegmentFeatureCompareFeatureReader(FileEncoding.UTF8)
                .Read(archive, FeatureType.Extract, new ZipArchiveFeatureReaderContext(zipArchiveMetadata)).Item1;
            extractsDbContext.InwinningRoadSegments.AddRange(extractRoadSegments
                .Select(x => x.Attributes.RoadSegmentId!.Value)
                .Distinct()
                .Select(x => new InwinningRoadSegment
                {
                    RoadSegmentId = x,
                    NisCode = nisCode,
                    Completed = false
                }));
        }

        await extractsDbContext.SaveChangesAsync();

        var sqsRequest = new MigrateDryRunRoadNetworkSqsRequest
        {
            MigrateRoadNetworkSqsRequest = new MigrateRoadNetworkSqsRequest
            {
                UploadId = new UploadId(Guid.NewGuid()),
                DownloadId = downloadId,
                Changes = translatedChanges.Select(ChangeRoadNetworkItem.Create).ToList(),
                IdenticalRoadSegmentIds = translatedChanges.GetIdenticalRoadSegmentIds(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = provenanceData
            },
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = provenanceData
        };
        await File.WriteAllTextAsync(".sqsRequest.json", JsonConvert.SerializeObject(sqsRequest, Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings()));

        var sqsLambdaRequest = new MigrateDryRunRoadNetworkSqsLambdaRequest("abc", sqsRequest);

        var sqsLambdaRequestHandler = new MigrateDryRunRoadNetworkSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            sp.GetRequiredService<IRoadRegistryContext>(),
            sp.GetRequiredService<IDocumentStore>(),
            new RoadNetworkRepository(sp.GetRequiredService<IDocumentStore>()),
            extractsDbContext,
            Mock.Of<IMediator>(),
            new NullLoggerFactory()
        );

        // Act
        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);
    }

    private IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .UseDefaultConfiguration(new HostingEnvironment())
            .Build();
        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging()
            .AddHttpClient()
            .AddSingleton<SqsLambdaHandlerOptions>(new FakeSqsLambdaHandlerOptions())
            .AddSingleton<ICustomRetryPolicy>(new FakeRetryPolicy())
            .AddSingleton(TicketingMock.Object)
            .AddSingleton(Mock.Of<IIdempotentCommandHandler>())
            .AddSingleton(Mock.Of<IRoadRegistryContext>())
            .AddSingleton(Mock.Of<IExtractUploadFailedEmailClient>())
            ;

        services
            .AddMartenRoad(options => options
                .AddRoadNetworkTopologyProjection()
                .AddRoadAggregatesSnapshots()
                .ConfigureExtractDocuments()).Services
            .AddSingleton<IRoadNetworkIdGenerator>(new InMemoryRoadNetworkIdGenerator())
            ;

        var sp = services.BuildServiceProvider();
        return sp.CreateScope().ServiceProvider;
    }
}
