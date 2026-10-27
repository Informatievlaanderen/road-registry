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

// Two neighbouring municipalities are collected independently. This upload merges two of its roads into one and no
// longer needs the road node they met at, so it removes it. But the neighbour had already collected the upper of
// the two, which means the merge cannot stand: ResolveNeighbourMerges cuts the upload back to what the neighbour
// left, and what is left runs up to that road node again - the one the upload was on its way to removing
// (GAWR-7827).
//
//   far    (0,300)  o   where the neighbour's road ends now
//                   |
//   north  (0,200)  x   the border road node the neighbour merged away when it collected its road
//                   | ws-201, removed, with ws-300 - the neighbour's merge - lying over it
//   middle (0,100)  o   the road node this upload no longer needs, and removes
//                   | ws-200
//   south  (0,0)    o
//
// The upload brings ws-200 running all the way from south to north, with ws-201 merged into it. The cut takes the
// stretch from middle to north back off it, so ws-200 ends on middle after all.
//
// Without the road node being kept, the upload is refused for the road node it removed itself:
// "De eind wegknoop van het wegsegment ontbreekt." With it kept, the upload is collected, and the road node is
// let go afterwards all the same: ws-200 and ws-300 are then the only two roads at it, so they are merged.
[Collection(nameof(DockerFixtureCollection))]
public class GivenANeighbourCollectedHalfOfAMergedRoad : RoadNetworkIntegrationTest
{
    private static readonly RoadNodeId SouthNodeId = new(100);
    private static readonly RoadNodeId MiddleNodeId = new(101);
    private static readonly RoadNodeId NorthNodeId = new(102);
    private static readonly RoadNodeId FarNodeId = new(103);

    private static readonly RoadSegmentId LowerRoadSegmentId = new(200);
    private static readonly RoadSegmentId UpperRoadSegmentId = new(201);
    private static readonly RoadSegmentId NeighbourRoadSegmentId = new(300);

    public GivenANeighbourCollectedHalfOfAMergedRoad(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task ThenTheRoadNodeTheCutGivesBackIsKept()
    {
        // Arrange
        var (sp, command) = await GivenTheRoadNetworkAndTheUpload();

        // Act
        var handler = sp.GetRequiredService<MigrateRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new MigrateRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        PrintTicketErrorsIfAvailable(command.TicketId);

        GetTicketError(command.TicketId).Should().BeNull(
            "the road node the cut gives ws-200 back is the one this upload removed, and it has to stay");
    }

    [Fact]
    public async Task ThenWhatIsLeftOfTheRoadIsCollected()
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

        var roadSegment = await session.LoadAsync<RoadSegment>(StreamKeyFactory.Create(typeof(RoadSegment), LowerRoadSegmentId));

        // Collected, rather than left as it was because the upload was refused - short of which the geometry below
        // would be the one the road network already held.
        roadSegment!.HasMigrated().Should().BeTrue();

        // Cut back to what the neighbour left, which runs up to the road node the upload removed.
        var line = roadSegment.Geometry.Value.GetSingleLineString();
        line.StartPoint.Coordinate.Should().Be(new Coordinate(0, 0));
        line.EndPoint.Coordinate.Should().Be(new Coordinate(0, 100));

        // Only once the road is collected does the road network find the road node unnecessary after all: what is
        // left of the upload and the neighbour's road are the only two at it, so it merges those into one and lets
        // the road node go - which is what the upload was after to begin with.
        var roadNode = await session.LoadAsync<RoadNode>(StreamKeyFactory.Create(typeof(RoadNode), MiddleNodeId));
        roadNode!.IsRemoved.Should().BeTrue();
    }

    private async Task<(IServiceProvider, MigrateRoadNetworkSqsRequest)> GivenTheRoadNetworkAndTheUpload()
    {
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        await using (var session = store.LightweightSession())
        {
            AppendRoadNode(session, SouthNodeId, Point(0, 0));
            AppendRoadNode(session, MiddleNodeId, Point(0, 100));

            // Merged away by the neighbour when it collected its road, so it arrives removed.
            var northNode = AppendRoadNode(session, NorthNodeId, Point(0, 200));
            northNode.Remove(new FakeProvenance());
            session.Store(northNode);

            AppendRoadNode(session, FarNodeId, Point(0, 300));

            AppendRoadSegment(session, LowerRoadSegmentId, Line(0, 0, 0, 100), SouthNodeId, MiddleNodeId);

            // The upper half, taken over by the neighbour, so it is gone here.
            var upper = AppendRoadSegment(session, UpperRoadSegmentId, Line(0, 100, 0, 200), MiddleNodeId, NorthNodeId);
            upper.RemoveBecauseOfMigration(new FakeProvenance());
            session.Store(upper);

            // What the neighbour collected over it: a road segment with V2 attributes, which is what makes the road
            // network treat the stretch as already taken rather than as a road it still needs. It runs past the
            // border road node, which is what the neighbour's own merge did away with.
            AppendCollectedRoadSegment(session, NeighbourRoadSegmentId, Line(0, 100, 0, 200, 0, 300), MiddleNodeId, FarNodeId);

            await session.SaveChangesAsync();
        }

        // What the municipality uploads: the two roads as one, from south all the way to north, and the road node
        // they met at removed because it is not needed for that one road.
        var mergedGeometry = Line(0, 0, 0, 100, 0, 200);

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
                        Geometry = Point(0, 200).ToRoadNodeGeometry(),
                        Grensknoop = false
                    }
                },
                new ChangeRoadNetworkItem
                {
                    ModifyRoadNode = new ModifyRoadNodeChange
                    {
                        RoadNodeId = SouthNodeId,
                        Geometry = Point(0, 0).ToRoadNodeGeometry(),
                        Grensknoop = false
                    }
                },
                new ChangeRoadNetworkItem
                {
                    RemoveRoadNode = new RemoveRoadNodeChange
                    {
                        RoadNodeId = MiddleNodeId
                    }
                },
                new ChangeRoadNetworkItem
                {
                    ModifyRoadSegment = new ModifyRoadSegmentChange
                    {
                        RoadSegmentIdReference = new RoadSegmentIdReference(LowerRoadSegmentId),
                        Geometry = mergedGeometry,
                        GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
                        Status = RoadSegmentStatusV2.Gerealiseerd,
                        AccessRestriction = Attribute<RoadSegmentAccessRestrictionV2>(mergedGeometry),
                        Category = Attribute<RoadSegmentCategoryV2>(mergedGeometry),
                        Morphology = Attribute<RoadSegmentMorphologyV2>(mergedGeometry),
                        StreetNameId = Attribute<StreetNameLocalId>(mergedGeometry),
                        MaintenanceAuthorityId = Attribute<OrganizationId>(mergedGeometry),
                        SurfaceType = Attribute<RoadSegmentSurfaceTypeV2>(mergedGeometry),
                        CarTrafficDirection = Attribute<RoadSegmentTrafficDirection>(mergedGeometry),
                        BikeTrafficDirection = Attribute<RoadSegmentTrafficDirection>(mergedGeometry),
                        PedestrianTrafficDirection = Attribute<RoadSegmentPedestrianTrafficDirection>(mergedGeometry)
                    }
                },
                new ChangeRoadNetworkItem
                {
                    RemoveRoadSegment = new RemoveRoadSegmentChange
                    {
                        RoadSegmentId = UpperRoadSegmentId
                    }
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
        foreach (var roadSegmentId in new[] { LowerRoadSegmentId, UpperRoadSegmentId })
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

    private static RoadNode AppendRoadNode(IDocumentSession session, RoadNodeId roadNodeId, Point point)
    {
        var roadNode = RoadNode.CreateForMigration(roadNodeId, point.ToRoadNodeGeometry());
        session.Store(roadNode);
        session.Events.Append(StreamKeyFactory.Create(typeof(RoadNode), roadNodeId), new RoadNodeAdded
        {
            RoadNodeId = roadNodeId,
            Geometry = roadNode.Geometry
        });

        return roadNode;
    }

    private static RoadSegment AppendRoadSegment(IDocumentSession session, RoadSegmentId roadSegmentId, RoadSegmentGeometry geometry, RoadNodeId startNodeId, RoadNodeId endNodeId)
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

        return roadSegment;
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

    private static Point Point(double x, double y)
    {
        return new Point(x, y) { SRID = WellknownSrids.Lambert08 };
    }

    private static RoadSegmentGeometry Line(params double[] coordinates)
    {
        var points = new List<Coordinate>();
        for (var i = 0; i < coordinates.Length; i += 2)
        {
            points.Add(new Coordinate(coordinates[i], coordinates[i + 1]));
        }

        return new MultiLineString([new LineString(points.ToArray())])
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
