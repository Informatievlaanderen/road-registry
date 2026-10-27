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
using Microsoft.Extensions.Logging;
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

// Temp branch only, and based on main - so it runs the code tst runs, without PR #2162. Runs the second of the
// two GAWR-7826 uploads through the real migrate path against whatever road network the configured connection
// points at (tst), as a dry run: it reports and writes nothing.
//
// The two uploads are adjacent 500x500 tiles of municipality 44021. Upload 1 (the left tile) collects
// 1254566..1254570, 1254572 and 1254573 - the two shared roads 1254570 and 1254572 among them, in full. Merging
// 1254569 into 1254570 and 1254573 into 1254572 does away with the road nodes between them, 2275965 and 2275968,
// and the merged roads keep the shared identifiers because those are the longer of each pair.
//
// Upload 2 (the right tile) was prepared against the extract from before that, so it still names 2275965 and
// 2275968 and still brings 1254570 and 1254572 as the short stretches they were. It is refused with
//
//   WEGKNOOP 2275965 - De wegknoop met WK_OIDN 2275965 is verwijderd.
//   WEGKNOOP 2275968 - De wegknoop met WK_OIDN 2275968 is verwijderd.
//   WEGSEGMENT 1254574 (WS_TEMPID 44,45,46) - De start/eind wegknoop van het wegsegment ontbreekt.
//
// What this run is for: to see why ResolveNeighbourMerges does not account for 2275965 and 2275968, and where
// the third error comes from - 1254574 is an identifier neither upload carries, and (44,45,46) is all three of
// upload 2's road segments at once.
//
// Point the connection string at tst and run:
//
//   dotnet test test/RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests ^
//     --filter "FullyQualifiedName~ForDebuggingGawr7826"
public class ForDebuggingGawr7826
{
    private const string ZipPath = @"C:\Users\RikDePeuter\Downloads\ee8363c3b9ee486b9c083f6dd21b876b_upload2\ee8363c3b9ee486b9c083f6dd21b876b_upload2.zip";
    private const string DownloadIdValue = "ee8363c3b9ee486b9c083f6dd21b876b";

    // The zone this upload is for, so the extracts side can be seeded in memory. A dry run only reads it.
    private const string NisCode = "44021";
    private const string Operator = "OVO002949";

    // Everything either upload touches around the two road nodes that go missing, plus 1254574 - the identifier
    // the third error names, which neither archive carries.
    private static readonly int[] RoadSegmentsOfInterest = [1254566, 1254567, 1254568, 1254569, 1254570, 1254571, 1254572, 1254573, 1254574];
    private static readonly int[] RoadNodesOfInterest = [2275962, 2275963, 2275964, 2275965, 2275966, 2275967, 2275968];

    private readonly ITestOutputHelper _outputHelper;
    private Mock<ITicketing> TicketingMock { get; } = new();

    public ForDebuggingGawr7826(ITestOutputHelper outputHelper)
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
            new LoggerFactory([new TestOutputLoggerProvider(_outputHelper)])
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
        _outputHelper.WriteLine($"=== {translatedChanges.Count()} changes, in the order the domain walks them:");

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
                RoadRegistry.RoadNode.Changes.ModifyRoadNodeChange c => $"ModifyRoadNode {c.RoadNodeId} at {c.Geometry?.WKT}",
                RoadRegistry.RoadNode.Changes.RemoveRoadNodeChange c => $"RemoveRoadNode {c.RoadNodeId}",
                RoadRegistry.RoadSegment.Changes.ModifyRoadSegmentChange c => $"ModifyRoadSegment {c.RoadSegmentIdReference} {c.Geometry?.WKT}",
                RoadRegistry.RoadSegment.Changes.RemoveRoadSegmentChange c => $"RemoveRoadSegment {c.RoadSegmentId}",
                _ => change.GetType().Name
            };

            _outputHelper.WriteLine($"  [{index}] {text}");
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

    // Everything the domain logs, in the test output. ResolveNeighbourMerges and ResolveRemovalCorrections each
    // log the road node they took on, which is what decides what happens to the road segment running up to it.
    private sealed class TestOutputLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestOutputLoggerProvider(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public ILogger CreateLogger(string categoryName) => new TestOutputLogger(_outputHelper, categoryName);

        public void Dispose()
        {
        }

        private sealed class TestOutputLogger : ILogger
        {
            private readonly ITestOutputHelper _outputHelper;
            private readonly string _categoryName;

            public TestOutputLogger(ITestOutputHelper outputHelper, string categoryName)
            {
                _outputHelper = outputHelper;
                _categoryName = categoryName;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                try
                {
                    _outputHelper.WriteLine($"[{logLevel}] {_categoryName}: {formatter(state, exception)}");
                    if (exception is not null)
                    {
                        _outputHelper.WriteLine($"  {exception}");
                    }
                }
                catch
                {
                    // the test is already done writing
                }
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
            .AddLogging(builder => builder
                .SetMinimumLevel(LogLevel.Information)
                .AddProvider(new TestOutputLoggerProvider(_outputHelper)))
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
