namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenMigratingRoadNetwork;

using AutoFixture;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V1;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Events.V1;
using RoadRegistry.Tests.BackOffice;
using Xunit.Abstractions;

[Collection(nameof(DockerFixtureCollection))]
public class GivenRoadNetwork : RoadNetworkIntegrationTest
{
    public GivenRoadNetwork(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task GivenV1RoadSegment_WhenAddingSegmentWithPartialOverlap_ThenError()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        // existing road network
        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadSegment1 = RoadSegment.CreateForMigration(
            roadSegmentId: new RoadSegmentId(100),
            geometry: ((MultiLineString)new WKTReader().Read("MULTILINESTRING ((597971.1015208486 697818.2053713053, 597950.7802565437 697829.8443076871, 597940.273913426 697835.9022689406))")).WithSrid(WellknownSrids.Lambert08).ToRoadSegmentGeometry(),
            status: RoadSegmentStatusV2.Gerealiseerd,
            startNodeId: new RoadNodeId(100),
            endNodeId: new RoadNodeId(101)
        );
        session.Store(roadSegment1);
        {
            var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegment1.RoadSegmentId);
            var legacyEvent = new RoadSegmentAdded
            {
                RoadSegmentId = roadSegment1.RoadSegmentId,
                Geometry = roadSegment1.Geometry,
                StartNodeId = roadSegment1.StartNodeId!.Value,
                EndNodeId = roadSegment1.EndNodeId!.Value
            };
            session.Events.Append(streamKey, legacyEvent);
        }

        var roadNode1 = RoadNode.CreateForMigration(
            roadNodeId: roadSegment1.StartNodeId!.Value,
            geometry: roadSegment1.Geometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry()
        );
        session.Store(roadNode1);
        {
            var streamKey = StreamKeyFactory.Create(typeof(RoadNode), roadNode1.RoadNodeId);
            var legacyEvent = new RoadNodeAdded
            {
                RoadNodeId = roadNode1.RoadNodeId,
                Geometry = roadNode1.Geometry
            };
            session.Events.Append(streamKey, legacyEvent);
        }
        var roadNode2 = RoadNode.CreateForMigration(
            roadNodeId: roadSegment1.EndNodeId!.Value,
            geometry: roadSegment1.Geometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry()
        );
        session.Store(roadNode2);
        {
            var streamKey = StreamKeyFactory.Create(typeof(RoadNode), roadNode2.RoadNodeId);
            var legacyEvent = new RoadNodeAdded
            {
                RoadNodeId = roadNode2.RoadNodeId,
                Geometry = roadNode2.Geometry
            };
            session.Events.Append(streamKey, legacyEvent);
        }
        await session.SaveChangesAsync();

        // changes to road network
        var addSegmentGeometry = ((MultiLineString)new WKTReader().Read("MULTILINESTRING ((597980 697818, 597971.1015208486 697818.2053713053, 597950.7802565437 697829.8443076871, 597940.273913426 697835.9022689406, 597920 697835))")).WithSrid(WellknownSrids.Lambert08).ToRoadSegmentGeometry();

        var provenanceData = new RoadRegistryProvenanceData();
        var command = new MigrateRoadNetworkSqsRequest
        {
            UploadId = TestData.Fixture.Create<UploadId>(),
            DownloadId = TestData.Fixture.Create<DownloadId>(),
            TicketId = TestData.Fixture.Create<TicketId>(),
            Changes =
            [
                new ChangeRoadNetworkItem
                {
                    AddRoadNode = new AddRoadNodeChange
                    {
                        TemporaryId = new RoadNodeId(1),
                        Geometry = addSegmentGeometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry()
                    }
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadNode = new AddRoadNodeChange
                    {
                        TemporaryId = new RoadNodeId(2),
                        Geometry = addSegmentGeometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry()
                    }
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadSegment = (TestData.AddSegment1 with
                    {
                        RoadSegmentIdReference = new RoadSegmentIdReference(new RoadSegmentId(1)),
                        Geometry = addSegmentGeometry,
                        Status = RoadSegmentStatusV2.Gerealiseerd
                    }).WithDynamicAttributePositionsOnEntireGeometryLength()
                }
            ],
            ProvenanceData = provenanceData
        };

        await using var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        await AddExtractDownloadAndUpload(extractsDbContext, command);

        var nisCode = "12345";
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.DownloadId,
            NisCode = nisCode,
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = roadSegment1.RoadSegmentId,
            NisCode = nisCode,
            Completed = false
        });
        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var handler = sp.GetRequiredService<MigrateRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new MigrateRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        TicketingMock.VerifyThatTicketHasError("RoadSegmentPartiallyOverlapsWithAnotherRoadSegment");
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton(new FakeExtractsDbContextFactory().CreateDbContext())
            .AddScoped<MigrateRoadNetworkSqsLambdaRequestHandler>();
    }

    private async Task AddExtractDownloadAndUpload(ExtractsDbContext extractsDbContext, MigrateRoadNetworkSqsRequest command)
    {
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = command.DownloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = TestData.Fixture.Create<ExtractRequestId>()
        });
        extractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = command.UploadId,
            DownloadId = command.DownloadId,
            UploadedOn = DateTimeOffset.UtcNow,
            Status = ExtractUploadStatus.Processing,
            TicketId = command.TicketId
        });
        await extractsDbContext.SaveChangesAsync();
    }
}
