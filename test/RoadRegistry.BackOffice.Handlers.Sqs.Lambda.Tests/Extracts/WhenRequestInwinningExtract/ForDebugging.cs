namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenRequestInwinningExtract;

using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestExtract;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestInwinningExtract;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.ZipArchiveWriters;
using RoadRegistry.Extracts.ZipArchiveWriters.Writers.Inwinning;
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
    private Mock<ITicketing> TicketingMock { get; } = new();

    public ForDebugging(ITestOutputHelper outputHelper)
    {
    }

    [Fact(Skip = "For debugging purposes only")]
    public async Task WithCustomRequestOnActualServer()
    {
        // Arrange
        var downloadId = new DownloadId(Guid.Parse("96652eb6-2527-41a7-bc59-241fab70e3aa"));
        var nisCode = "71016";

        var sp = BuildServiceProvider();

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        var inwinningszone = extractsDbContext.Inwinningszones.Single(x => x.NisCode == nisCode);

        var request = new RequestExtractData(
            ExtractRequestId.FromString("9286890be2fe6fae5ad990664ba762f9b4165078a6d9b422130e64b2aab8fc46"),
            downloadId,
            (MultiPolygon)inwinningszone.Contour,
            "Rik test",
            false,
            $"INWINNING_{nisCode}"
        );
        var provenanceData = new RoadRegistryProvenanceData(operatorName: inwinningszone.Operator);

        // Act
        var sqsRequest = new RequestInwinningExtractSqsRequest
        {
            ExtractRequestId = request.ExtractRequestId,
            DownloadId = request.DownloadId,
            Contour = request.Contour.ToExtractGeometry(),
            Description = request.Description,
            IsInformative = request.IsInformative,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = provenanceData,
            NisCode = nisCode
        };

        var sqsLambdaRequest = new RequestInwinningExtractSqsLambdaRequest(sqsRequest.DownloadId.ToString(), sqsRequest);

        var sqsLambdaRequestHandler = new RequestInwinningExtractSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            sp.GetRequiredService<IRoadRegistryContext>(),
            extractsDbContext,
            new ExtractRequester(
                extractsDbContext,
                new RoadNetworkExtractDownloadsBlobClient(Mock.Of<IBlobClient>()),
                new RoadNetworkExtractArchiveAssembler(sp),
                new NullLoggerFactory()),
            new RoadSegmentFeatureCompareFeatureReader(FileEncoding.UTF8),
            new NullLoggerFactory()
        );

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
            .AddLogging();

        services
            .AddSingleton<SqsLambdaHandlerOptions>(new FakeSqsLambdaHandlerOptions())
            .AddSingleton<ICustomRetryPolicy>(new FakeRetryPolicy())
            .AddSingleton(TicketingMock.Object)
            .AddSingleton(Mock.Of<IIdempotentCommandHandler>())
            .AddSingleton(Mock.Of<IRoadRegistryContext>())
            .AddSingleton(Mock.Of<IExtractUploadFailedEmailClient>())
            .AddSingleton(new ZipArchiveWriterOptions())
            .AddScoped(sp => new RoadNetworkExtractArchiveAssemblerForDomainV2(
                new RecyclableMemoryStreamManager(),
                new ZipArchiveWriterFactoryForDomainV2(
                    new RoadNetworkExtractZipArchiveWriter(
                        sp.GetRequiredService<ZipArchiveWriterOptions>(),
                        new RecyclableMemoryStreamManager(),
                        FileEncoding.UTF8,
                        sp.GetRequiredService<ILoggerFactory>()
                    )),
                sp.GetRequiredService<IDocumentStore>(),
                new RoadNetworkRepository(sp.GetRequiredService<IDocumentStore>()),
                sp.GetRequiredService<Func<EditorContext>>(),
                new NullLoggerFactory()))
            ;

        services
            .AddMartenRoad(options => { options.ConfigureExtractDocuments(); }).Services
            .AddSingleton<IRoadNetworkIdGenerator>(new InMemoryRoadNetworkIdGenerator())
            .AddExtractsDbContext(QueryTrackingBehavior.TrackAll)
            .AddEditorContext();

        var sp = services.BuildServiceProvider();
        return sp.CreateScope().ServiceProvider;
    }
}
