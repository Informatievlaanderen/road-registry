namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenMigratingRoadNetwork;

using AutoFixture;
using FluentAssertions;
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
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V1;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.Tests.BackOffice;
using Xunit.Abstractions;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// Two adjacent inwinning tiles of the same municipality, collected one after the other. The first tile collected
// the two roads that cross into the second one, 1254570 and 1254572, along with the roads before them - and in
// doing so removed the two road nodes where they met, 2275965 and 2275968.
//
// The second tile was prepared against the extract from before that, so it still knows those two road nodes and
// still has the two roads as the stretches they were. Its uploader collected the three roads it has as one, which
// the feature compare hands over as a road drawn anew with the three it replaces removed.
//
//   2275962  o                       2275965                     2275966
//            |                          x---------------------------o
//   2275964  o--------------------------                            |  1254571
//            |  1254570 and 1254572, as the first tile left them    |
//            |                          x---------------------------o
//                                    2275968                     2275967
//
// Both ends of the road the second tile draws run up to a road node the first tile merged away, so both have to be
// cut back to what the first tile left - the GAWR-7826 shape. What is left is the stretch between 2275966 and
// 2275967, the only one the first tile did not collect.
//
// Short of that the upload is refused with
//
//   De wegknoop met WK_OIDN 2275965 is verwijderd.
//   De wegknoop met WK_OIDN 2275968 is verwijderd.
//   De start/eind wegknoop van het wegsegment ontbreekt.
//
// The coordinates are the ones from tst: the road network holds a road node it has collected on the centimetre and
// one it has not at the coordinate V1 gave it, and the upload works on the centimetre throughout.
[Collection(nameof(DockerFixtureCollection))]
public class GivenANeighbourMergedAwayTheRoadNodesAtBothEnds : RoadNetworkIntegrationTest
{
    private static readonly RoadNodeId WestNodeId = new(2275962);
    private static readonly RoadNodeId CentreNodeId = new(2275964);
    private static readonly RoadNodeId NorthNodeId = new(2275965);
    private static readonly RoadNodeId EastNorthNodeId = new(2275966);
    private static readonly RoadNodeId EastSouthNodeId = new(2275967);
    private static readonly RoadNodeId SouthNodeId = new(2275968);

    private static readonly RoadSegmentId WestRoadSegmentId = new(1254567);
    private static readonly RoadSegmentId NorthRoadSegmentId = new(1254570);
    private static readonly RoadSegmentId EastRoadSegmentId = new(1254571);
    private static readonly RoadSegmentId SouthRoadSegmentId = new(1254572);

    // What the first tile collected, on the centimetre.
    private static readonly Coordinate West = new(608470.62, 697297);
    private static readonly Coordinate Centre = new(608476.54, 697291.29);
    private static readonly Coordinate EastNorth = new(608514.22, 697293.41);
    private static readonly Coordinate EastSouth = new(608514.22, 697287.06);

    // The two road nodes the first tile removed before ever collecting them, so they still sit where V1 put them.
    private static readonly Coordinate North = new(608482.2576114626, 697297.0045951791);
    private static readonly Coordinate South = new(608482.4703343377, 697288.7496886486);

    // And where the second tile's upload has them, which is the same two on the centimetre.
    private static readonly Coordinate UploadedNorth = new(608482.26, 697297);
    private static readonly Coordinate UploadedSouth = new(608482.47, 697288.75);

    // The two roads the neighbouring tile collected run through where those road nodes were: each is the merge
    // of the stretch before it with the stretch after, which is what did away with the road node between them.
    private static RoadSegmentGeometry NorthRoad => Line(Centre, UploadedNorth, EastNorth);
    private static RoadSegmentGeometry SouthRoad => Line(EastSouth, UploadedSouth, Centre);

    public GivenANeighbourMergedAwayTheRoadNodesAtBothEnds(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task ThenTheUploadIsAccepted()
    {
        // Arrange
        var (sp, command) = await GivenTheRoadNetworkAndTheUpload();

        // Act
        var handler = sp.GetRequiredService<MigrateRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new MigrateRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        PrintTicketErrorsIfAvailable(command.TicketId);

        GetTicketError(command.TicketId).Should().BeNull(
            "both ends of the road this tile draws run into what the neighbouring tile already collected");
    }

    [Fact]
    public async Task ThenWhatTheNeighbouringTileCollectedIsLeftAlone()
    {
        // Arrange
        var (sp, command) = await GivenTheRoadNetworkAndTheUpload();

        // Act
        var handler = sp.GetRequiredService<MigrateRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new MigrateRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        PrintTicketErrorsIfAvailable(command.TicketId);

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        // The stretch between the two road nodes the cuts land on is the only one this tile still had to collect,
        // and the only one it replaces. Short of the cuts the upload is refused and nothing of it is on record,
        // so this is what says the tile was collected at all.
        var replaced = await session.LoadAsync<RoadSegment>(StreamKeyFactory.Create(typeof(RoadSegment), EastRoadSegmentId));
        replaced!.IsRemoved.Should().BeTrue("this tile draws its own road over it");

        // What the neighbouring tile collected is still there, from the centre out. This upload says to remove
        // both of those roads, believing the road it draws replaces them, but the cuts handed those stretches
        // back: they are the neighbour's and not this upload's to remove.
        var north = await session.LoadAsync<RoadSegment>(StreamKeyFactory.Create(typeof(RoadSegment), NorthRoadSegmentId));
        north!.IsRemoved.Should().BeFalse("it is what the neighbouring tile collected");

        var northCoordinates = north.Geometry.Value.GetSingleLineString().Coordinates;
        northCoordinates[0].Should().Be(Centre);
        northCoordinates.Should().Contain(UploadedNorth, "the road still runs over the road node the neighbour merged away");

        // Once this tile's stretch is collected, the road node the cut left the drawn road running up to has only
        // those two roads at it, so the road network merges them and 1254570 grows by what this tile brought.
        north.Geometry.Value.Length.Should().BeApproximately(NorthRoad.Value.Length + EastNorth.Distance(EastSouth), 0.01);

        var south = await session.LoadAsync<RoadSegment>(StreamKeyFactory.Create(typeof(RoadSegment), SouthRoadSegmentId));
        south!.IsRemoved.Should().BeFalse("it is what the neighbouring tile collected");
        south.Geometry.Value.Length.Should().BeApproximately(SouthRoad.Value.Length, 0.01);
    }

    private async Task<(IServiceProvider, MigrateRoadNetworkSqsRequest)> GivenTheRoadNetworkAndTheUpload()
    {
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        await using (var session = store.LightweightSession())
        {
            AppendCollectedRoadNode(session, WestNodeId, West);
            AppendCollectedRoadNode(session, CentreNodeId, Centre);
            AppendCollectedRoadNode(session, EastNorthNodeId, EastNorth);
            AppendCollectedRoadNode(session, EastSouthNodeId, EastSouth);

            // Removed by the neighbouring tile before either was ever collected.
            foreach (var (roadNodeId, coordinate) in new[] { (NorthNodeId, North), (SouthNodeId, South) })
            {
                var roadNode = AppendRoadNode(session, roadNodeId, coordinate);
                roadNode.RemoveBecauseOfMigration(new FakeProvenance());
                session.Store(roadNode);
            }

            // Keeps the centre a road node of its own rather than one the road network would do away with.
            AppendCollectedRoadSegment(session, WestRoadSegmentId, Line(Centre, West), CentreNodeId, WestNodeId);

            // The two roads the neighbouring tile collected, running from the centre all the way east: it merged
            // the stretches before them into these, which is what did away with the two road nodes between.
            AppendCollectedRoadSegment(session, NorthRoadSegmentId, NorthRoad, CentreNodeId, EastNorthNodeId);
            AppendCollectedRoadSegment(session, SouthRoadSegmentId, SouthRoad, EastSouthNodeId, CentreNodeId);

            // The only stretch the neighbouring tile does not cover, so it is still as V1 left it.
            AppendRoadSegment(session, EastRoadSegmentId, Line(EastNorth, EastSouth), EastNorthNodeId, EastSouthNodeId);

            await session.SaveChangesAsync();
        }

        // What the uploader collected as one road, from the one road node the neighbour merged away, around the
        // east side, to the other.
        var drawnAnew = Line(UploadedNorth, EastNorth, EastSouth, UploadedSouth);

        var provenanceData = new RoadRegistryProvenanceData(reason: "Inwinning 12345");
        var command = new MigrateRoadNetworkSqsRequest
        {
            UploadId = TestData.Fixture.Create<UploadId>(),
            DownloadId = TestData.Fixture.Create<DownloadId>(),
            TicketId = TestData.Fixture.Create<TicketId>(),
            Changes =
            [
                new ChangeRoadNetworkItem
                {
                    ModifyRoadNode = new ModifyRoadNodeChange
                    {
                        RoadNodeId = NorthNodeId,
                        Geometry = new Point(UploadedNorth) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry(),
                        Grensknoop = false
                    }
                },
                new ChangeRoadNetworkItem
                {
                    RemoveRoadNode = new RemoveRoadNodeChange { RoadNodeId = EastNorthNodeId }
                },
                new ChangeRoadNetworkItem
                {
                    RemoveRoadNode = new RemoveRoadNodeChange { RoadNodeId = EastSouthNodeId }
                },
                new ChangeRoadNetworkItem
                {
                    ModifyRoadNode = new ModifyRoadNodeChange
                    {
                        RoadNodeId = SouthNodeId,
                        Geometry = new Point(UploadedSouth) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry(),
                        Grensknoop = false
                    }
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadSegment = new AddRoadSegmentChange
                    {
                        RoadSegmentIdReference = new RoadSegmentIdReference(new RoadSegmentId(1254574)),
                        Geometry = drawnAnew,
                        GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
                        Status = RoadSegmentStatusV2.Gerealiseerd,
                        AccessRestriction = Attribute<RoadSegmentAccessRestrictionV2>(drawnAnew),
                        Category = Attribute<RoadSegmentCategoryV2>(drawnAnew),
                        Morphology = Attribute<RoadSegmentMorphologyV2>(drawnAnew),
                        StreetNameId = Attribute<StreetNameLocalId>(drawnAnew),
                        MaintenanceAuthorityId = Attribute<OrganizationId>(drawnAnew),
                        SurfaceType = Attribute<RoadSegmentSurfaceTypeV2>(drawnAnew),
                        CarTrafficDirection = Attribute<RoadSegmentTrafficDirection>(drawnAnew),
                        BikeTrafficDirection = Attribute<RoadSegmentTrafficDirection>(drawnAnew),
                        PedestrianTrafficDirection = Attribute<RoadSegmentPedestrianTrafficDirection>(drawnAnew)
                    }
                },
                new ChangeRoadNetworkItem
                {
                    RemoveRoadSegment = new RemoveRoadSegmentChange { RoadSegmentId = NorthRoadSegmentId }
                },
                new ChangeRoadNetworkItem
                {
                    RemoveRoadSegment = new RemoveRoadSegmentChange { RoadSegmentId = EastRoadSegmentId }
                },
                new ChangeRoadNetworkItem
                {
                    RemoveRoadSegment = new RemoveRoadSegmentChange { RoadSegmentId = SouthRoadSegmentId }
                }
            ],
            ProvenanceData = provenanceData
        };

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        await AddExtractDownloadAndUpload(extractsDbContext, command);

        const string nisCode = "12345";
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.DownloadId,
            NisCode = nisCode,
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        foreach (var roadSegmentId in new[] { NorthRoadSegmentId, EastRoadSegmentId, SouthRoadSegmentId })
        {
            extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
            {
                RoadSegmentId = roadSegmentId,
                NisCode = nisCode,
                Completed = false
            });
        }

        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

        return (sp, command);
    }

    private static RoadNode AppendRoadNode(IDocumentSession session, RoadNodeId roadNodeId, Coordinate coordinate)
    {
        var roadNode = RoadNode.CreateForMigration(roadNodeId, Point(coordinate).ToRoadNodeGeometry());
        session.Store(roadNode);
        session.Events.Append(StreamKeyFactory.Create(typeof(RoadNode), roadNodeId), new RoadNodeAdded
        {
            RoadNodeId = roadNodeId,
            Geometry = roadNode.Geometry
        });

        return roadNode;
    }

    private static void AppendCollectedRoadNode(IDocumentSession session, RoadNodeId roadNodeId, Coordinate coordinate)
    {
        var roadNode = AppendRoadNode(session, roadNodeId, coordinate);
        roadNode.Migrate(new MigrateRoadNodeChange
        {
            RoadNodeId = roadNodeId,
            Geometry = Point(coordinate).ToRoadNodeGeometry(),
            Grensknoop = false
        }, new FakeProvenance());
        session.Store(roadNode.WithoutChanges());
    }

    private static void AppendRoadSegment(IDocumentSession session, RoadSegmentId roadSegmentId, RoadSegmentGeometry geometry, RoadNodeId startNodeId, RoadNodeId endNodeId)
    {
        var roadSegment = RoadSegment.CreateForMigration(roadSegmentId, geometry, RoadSegmentStatusV2.Gerealiseerd, startNodeId, endNodeId);
        session.Store(roadSegment);
        session.Events.Append(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId), new RoadSegmentAdded
        {
            RoadSegmentId = roadSegmentId,
            Geometry = geometry,
            StartNodeId = startNodeId,
            EndNodeId = endNodeId
        });
    }

    private void AppendCollectedRoadSegment(IDocumentSession session, RoadSegmentId roadSegmentId, RoadSegmentGeometry geometry, RoadNodeId startNodeId, RoadNodeId endNodeId)
    {
        var roadSegmentWasAdded = TestData.Segment1Added with
        {
            RoadSegmentId = roadSegmentId,
            Geometry = geometry,
            Status = RoadSegmentStatusV2.Gerealiseerd,
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            AccessRestriction = Attribute<RoadSegmentAccessRestrictionV2>(geometry),
            Category = Attribute<RoadSegmentCategoryV2>(geometry),
            Morphology = Attribute<RoadSegmentMorphologyV2>(geometry),
            StreetNameId = Attribute<StreetNameLocalId>(geometry),
            MaintenanceAuthorityId = Attribute<OrganizationId>(geometry),
            SurfaceType = Attribute<RoadSegmentSurfaceTypeV2>(geometry),
            CarTrafficDirection = Attribute<RoadSegmentTrafficDirection>(geometry),
            BikeTrafficDirection = Attribute<RoadSegmentTrafficDirection>(geometry),
            PedestrianTrafficDirection = Attribute<RoadSegmentPedestrianTrafficDirection>(geometry)
        };

        session.Store(RoadSegment.Create(roadSegmentWasAdded).WithoutChanges());
        session.Events.Append(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId), roadSegmentWasAdded);
    }

    private RoadSegmentDynamicAttributeValues<T> Attribute<T>(RoadSegmentGeometry geometry)
    {
        return new RoadSegmentDynamicAttributeValues<T>()
            .Add(TestData.Fixture.Create<T>(), geometry);
    }

    private static Point Point(Coordinate coordinate)
    {
        return new Point(coordinate) { SRID = WellknownSrids.Lambert08 };
    }

    private static RoadSegmentGeometry Line(params Coordinate[] coordinates)
    {
        return new MultiLineString([new LineString(coordinates)])
            .WithSrid(WellknownSrids.Lambert08)
            .ToRoadSegmentGeometry();
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
