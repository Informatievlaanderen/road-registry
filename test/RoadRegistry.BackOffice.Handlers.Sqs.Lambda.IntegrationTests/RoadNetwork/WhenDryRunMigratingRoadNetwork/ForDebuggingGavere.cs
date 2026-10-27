namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenDryRunMigratingRoadNetwork;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateDryRunRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using TicketingService.Abstractions;
using Xunit.Abstractions;

// Temp branch only. Runs the Gavere TEST upload through the real migrate path against whatever road network the
// configured connection points at (stg), as a dry run - it reports and writes nothing.
//
// The upload is refused with
//
//   WEGSEGMENT 930088 (WS_TEMPID 1538,1168) - De eind wegknoop van het wegsegment met WS_TEMPID (1538,1168) ontbreekt.
//
// and the point of this is to see which check actually raises it. Feature compare combines WS_TEMPID 1538
// (road segment 930088) and 1168 (833083) into one road:
//
//   (599276.45, 680787.45) -> (599194.33, 681119.62) -> (599172.97, 681210.19)
//        road node 1860175       road node 1860176         road node 1666166
//
// 1860176 is the one the merge does away with. Both other road nodes are in the archive, so the upload collects
// them; 1860175 and 1666166 are still at the coordinate V1 gave them in the register.
//
// Point the connection string at stg and run:
//
//   dotnet test test/RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests \
//     --filter "FullyQualifiedName~ForDebuggingGavere"
public class ForDebuggingGavere
{
    private const string ZipPath = @"C:\Users\RikDePeuter\Downloads\cae0af28d9da4391919f27092f8b55a7_stg_Gavere TEST\cae0af28d9da4391919f27092f8b55a7_stg_Gavere TEST.zip";
    private const string DownloadIdValue = "68bbe07a400c4cf6bd0c31c2dd1be98f";

    // What the first run reported for this download, so the extracts side can be seeded in memory.
    private const string NisCode = "44020";
    private const string Operator = "OVO002949";

    // The road segments and road nodes the failure is about, so the dump below stays readable.
    private static readonly int[] RoadSegmentsOfInterest = [930088, 833083];
    private static readonly int[] RoadNodesOfInterest = [1860175, 1860176, 1666166];

    private readonly ITestOutputHelper _outputHelper;
    private Mock<ITicketing> TicketingMock { get; } = new();

    public ForDebuggingGavere(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task RunTheUpload()
    {
        // Arrange
        var downloadId = DownloadId.Parse(DownloadIdValue);

        var sp = BuildServiceProvider();

        // In memory, so the run touches nothing on the extracts side. Marten still points at the real road network,
        // which is what we are here to read, and a dry run does not write to it.
        var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
        var uploadId = new UploadId(Guid.NewGuid());
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = downloadId,
            NisCode = NisCode,
            Operator = Operator,
            Contour = Polygon.Empty,
            Completed = false
        });
        extractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = uploadId.ToGuid(),
            DownloadId = downloadId,
            UploadedOn = DateTimeOffset.UtcNow,
            Status = ExtractUploadStatus.Processing,
            TicketId = Guid.NewGuid()
        });

        var provenanceData = new RoadRegistryProvenanceData(operatorName: Operator);

        TranslatedChanges translatedChanges;
        using (var fileStream = File.OpenRead(ZipPath))
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient(nameof(GrbOgcApiFeaturesDownloader));
            const string baseUrl = "https://geo.api.vlaanderen.be/GRB/ogc/features/v1";

            var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(
                grbOgcApiFeaturesDownloader: new GrbOgcApiFeaturesDownloader(client, baseUrl));
            var archive = new ZipArchive(fileStream);

            var zipArchiveMetadata = ZipArchiveMetadata.Empty.WithInwinning();
            translatedChanges = await translator.TranslateAsync(archive, zipArchiveMetadata, CancellationToken.None);

            // Which road segments this inwinning is about, the way the real run knows them.
            var extractRoadSegments = new RoadSegmentFeatureCompareFeatureReader(FileEncoding.UTF8)
                .Read(archive, FeatureType.Extract, new ZipArchiveFeatureReaderContext(zipArchiveMetadata)).Item1;
            extractsDbContext.InwinningRoadSegments.AddRange(extractRoadSegments
                .Select(x => x.Attributes.RoadSegmentId!.Value)
                .Distinct()
                .Select(x => new InwinningRoadSegment
                {
                    RoadSegmentId = x,
                    NisCode = NisCode,
                    Completed = false
                }));
        }

        await extractsDbContext.SaveChangesAsync();

        DumpChangesOfInterest(translatedChanges);
        await DumpWhatTheRegisterHolds(sp);

        var sqsRequest = new MigrateDryRunRoadNetworkSqsRequest
        {
            MigrateRoadNetworkSqsRequest = new MigrateRoadNetworkSqsRequest
            {
                UploadId = uploadId,
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

        var sqsLambdaRequestHandler = new MigrateDryRunRoadNetworkSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            sp.GetRequiredService<IDocumentStore>(),
            new RoadNetworkRepository(sp.GetRequiredService<IDocumentStore>()),
            extractsDbContext,
            Mock.Of<IMediator>(),
            new NullLoggerFactory()
        );

        // Act
        try
        {
            await sqsLambdaRequestHandler.Handle(new MigrateDryRunRoadNetworkSqsLambdaRequest("debug", sqsRequest), CancellationToken.None);
        }
        catch (Exception ex)
        {
            _outputHelper.WriteLine($"=== Handle threw: {ex}");
        }

        // Assert
        DumpTicketOutcome();
    }

    // What the archive turned into, for the road segments and road nodes the failure is about, and in the order the
    // domain will walk them - which is what decides whether a road node is already migrated when the road segment
    // that runs up to it is checked.
    private void DumpChangesOfInterest(TranslatedChanges translatedChanges)
    {
        _outputHelper.WriteLine($"=== {translatedChanges.Count()} changes, the ones of interest in order:");

        var index = 0;
        foreach (var change in translatedChanges)
        {
            index++;

            var text = change switch
            {
                RoadRegistry.RoadNode.Changes.ModifyRoadNodeChange c when RoadNodesOfInterest.Contains(c.RoadNodeId.ToInt32())
                    => $"ModifyRoadNode {c.RoadNodeId} at {c.Geometry?.WKT}",
                RoadRegistry.RoadNode.Changes.RemoveRoadNodeChange c when RoadNodesOfInterest.Contains(c.RoadNodeId.ToInt32())
                    => $"RemoveRoadNode {c.RoadNodeId}",
                RoadRegistry.RoadSegment.Changes.ModifyRoadSegmentChange c when RoadSegmentsOfInterest.Contains(c.RoadSegmentIdReference.RoadSegmentId.ToInt32())
                    => $"ModifyRoadSegment {c.RoadSegmentIdReference} {c.Geometry?.WKT}",
                RoadRegistry.RoadSegment.Changes.RemoveRoadSegmentChange c when RoadSegmentsOfInterest.Contains(c.RoadSegmentId.ToInt32())
                    => $"RemoveRoadSegment {c.RoadSegmentId}",
                _ => null
            };

            if (text is not null)
            {
                _outputHelper.WriteLine($"  [{index}] {text}");
            }
        }
    }

    // The coordinates the domain will compare the upload against. An upload is read in on the centimetre; a road
    // node the register has not migrated yet is not, and FindStartEndNodes matches exactly.
    private async Task DumpWhatTheRegisterHolds(IServiceProvider sp)
    {
        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        _outputHelper.WriteLine("=== What the register holds:");

        var roadNodes = await session.LoadManyAsync(RoadNodesOfInterest.Select(x => new RoadNodeId(x)));
        foreach (var roadNode in roadNodes)
        {
            _outputHelper.WriteLine(
                $"  road node {roadNode.RoadNodeId}: {roadNode.Geometry?.WKT}"
                + $" removed={roadNode.IsRemoved} removedBecauseOfMigration={roadNode.IsRemovedBecauseOfMigration}"
                + $" migrated={roadNode.HasMigrated()}");
        }

        var missingRoadNodes = RoadNodesOfInterest.Except(roadNodes.Select(x => x.RoadNodeId.ToInt32())).ToArray();
        if (missingRoadNodes.Any())
        {
            _outputHelper.WriteLine($"  road nodes with no document at all: {string.Join(", ", missingRoadNodes)}");
        }

        var roadSegments = await session.LoadManyAsync(RoadSegmentsOfInterest.Select(x => new RoadSegmentId(x)));
        foreach (var roadSegment in roadSegments)
        {
            var line = roadSegment.Geometry?.Value.GetSingleLineString();
            _outputHelper.WriteLine(
                $"  road segment {roadSegment.RoadSegmentId}: start node {roadSegment.StartNodeId} end node {roadSegment.EndNodeId}"
                + $" status={roadSegment.Status} removed={roadSegment.IsRemoved} migrated={roadSegment.HasMigrated()}");
            if (line is not null)
            {
                _outputHelper.WriteLine($"    from {line.StartPoint.Coordinate} to {line.EndPoint.Coordinate}");
            }
        }
    }

    private void DumpTicketOutcome()
    {
        foreach (var invocation in TicketingMock.Invocations)
        {
            if (invocation.Method.Name == nameof(ITicketing.Error))
            {
                var ticketError = (TicketError)invocation.Arguments[1];
                _outputHelper.WriteLine("=== Ticket errors:");
                foreach (var error in ticketError.Errors ?? [])
                {
                    _outputHelper.WriteLine($"  {error.ErrorCode}: {error.ErrorMessage}");
                }
            }
            else if (invocation.Method.Name == nameof(ITicketing.Complete))
            {
                _outputHelper.WriteLine("=== Ticket completed, no problems.");
            }
        }
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
            .AddExtractsDbContext(QueryTrackingBehavior.TrackAll)
            ;

        var sp = services.BuildServiceProvider();
        return sp.CreateScope().ServiceProvider;
    }
}
