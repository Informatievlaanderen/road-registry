namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MigrateRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// Where the cut leaves the road, for an upload a neighbouring inwinning took only part of (see NeighbourMergeTests for
// the story). The stretch that is left has to start exactly on the road node the two roads meet at, because that is
// how the road network looks a road node up - by an exact coordinate, with no tolerance at all:
//
//     return candidates.FirstOrDefault(x => x.Geometry.Value.EqualsExact(point));
//
// The cut works to a tolerance instead: it accepts the road the neighbour collected as lining up when its end lies
// within a centimetre of the uploaded road, and it cuts at the point the projection lands on rather than at the road
// node itself. A hair of difference is enough for the road that is left to have no end node at all.
//
// The roads here run diagonally, which is what makes the difference show. Along a road that runs straight north the
// interpolation has nothing to divide and lands on the road node exactly; along one that runs at an angle it does
// not, and that is every road in the register.
//
//   north (0,0)      --- the far end of what the neighbour collected
//     \ ws-100
//   border (150,150.004) --- the road node both uploads share, a few millimetres off the line the upload drew
//     \ ws-200
//   south (200,200)  --- the far end of what this upload collects
//
// The upload names the road node at middle (100,100), which the neighbour's merge did away with, and brings the road
// from there all the way to south. The stretch from middle to border was already collected.
public class NeighbourMergeCutBackTests : RoadNetworkTestBase
{
    private static readonly Coordinate North = new(0, 0);
    private static readonly Coordinate Middle = new(100, 100);
    private static readonly Coordinate Border = new(150, 150.004);
    private static readonly Coordinate South = new(200, 200);

    private static readonly RoadNodeId NorthNodeId = new(1);
    private static readonly RoadNodeId MiddleNodeId = new(2);
    private static readonly RoadNodeId BorderNodeId = new(3);
    private static readonly RoadNodeId SouthNodeId = new(4);

    private static readonly RoadSegmentId NeighbourSegmentId = new(100);
    private static readonly RoadSegmentId UploadedSegmentId = new(200);

    [Fact]
    public void WhenTheRoadIsCutBack_ThenWhatIsLeftStillFindsItsRoadNodes()
    {
        var roadNetwork = BuildNetworkWithCollectedRoad();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().BeEmpty();
    }

    [Fact]
    public void WhenTheRoadIsCutBack_ThenItStartsOnTheRoadNodeTheTwoRoadsShare()
    {
        var roadNetwork = BuildNetworkWithCollectedRoad();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        // Asserted first, so what follows cannot pass on a road the migration left untouched.
        result.Problems.Select(x => x.Reason).Should().BeEmpty();

        // Exactly, not nearly: the road network has no tolerance to spare when it looks a road node up.
        var collected = roadNetwork.RoadSegments[UploadedSegmentId];
        collected.Geometry.Value.GetSingleLineString().StartPoint.Coordinate.Should().Be(Border);
        collected.Geometry.Value.Length.Should().BeApproximately(Border.Distance(South), 0.001);
    }

    // The network as the second municipality finds it: the road the neighbour collected, running up to the road node
    // the two share, and the road this upload is about on the other side of it. The road node between the neighbour's
    // two roads is gone, merged away by their inwinning.
    private ScopedRoadNetwork BuildNetworkWithCollectedRoad()
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes:
            [
                RoadNode.CreateForMigration(NorthNodeId, Node(North)),
                RoadNode.CreateForMigration(BorderNodeId, Node(Border)),
                RoadNode.CreateForMigration(SouthNodeId, Node(South)),
                BuildRemovedRoadNode(MiddleNodeId, Middle)
            ],
            roadSegments:
            [
                RoadSegment.CreateForMigration(UploadedSegmentId, Line(Border, South).ToRoadSegmentGeometry(), RoadSegmentStatusV2.Gerealiseerd, BorderNodeId, SouthNodeId),
                BuildCollectedRoadSegment(NeighbourSegmentId, North, Border, NorthNodeId, BorderNodeId)
            ]).WithoutChanges();
    }

    // What the municipality uploads: the road node its neighbour did away with, the ones it did not, and its road
    // running the whole way from there to south - the first stretch of which is already collected.
    private RoadNetworkChanges Upload()
    {
        var geometry = Line(Middle, South);

        var changes = RoadNetworkChanges.Start()
            .WithProvenance(new FakeProvenance())
            .Add(new ModifyRoadSegmentChange
            {
                RoadSegmentIdReference = new RoadSegmentIdReference(UploadedSegmentId),
                Geometry = geometry.ToRoadSegmentGeometry(),
                GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
                Status = RoadSegmentStatusV2.Gerealiseerd,
                AccessRestriction = Attribute<RoadSegmentAccessRestrictionV2>(geometry),
                Category = Attribute<RoadSegmentCategoryV2>(geometry),
                Morphology = Attribute<RoadSegmentMorphologyV2>(geometry),
                StreetNameId = Attribute<StreetNameLocalId>(geometry),
                MaintenanceAuthorityId = Attribute<OrganizationId>(geometry),
                SurfaceType = Attribute<RoadSegmentSurfaceTypeV2>(geometry),
                CarTrafficDirection = Attribute<RoadSegmentTrafficDirection>(geometry),
                BikeTrafficDirection = Attribute<RoadSegmentTrafficDirection>(geometry),
                PedestrianTrafficDirection = Attribute<RoadSegmentPedestrianTrafficDirection>(geometry)
            });

        foreach (var (roadNodeId, coordinate, grensknoop) in new[]
                 {
                     (MiddleNodeId, Middle, false),
                     // A grensknoop, so the road node the two uploads share stays where it is rather than being
                     // merged away: what this is about is the road that is left, not what becomes of it afterwards.
                     (BorderNodeId, Border, true),
                     (SouthNodeId, South, false)
                 })
        {
            changes = changes.Add(new ModifyRoadNodeChange
            {
                RoadNodeId = roadNodeId,
                Geometry = Node(coordinate),
                Grensknoop = grensknoop
            });
        }

        return changes;
    }

    // What the neighbour already collected: a road segment with V2 attributes. Without them the road network treats
    // the road node between two roads as one it still needs, and nothing would ever be merged.
    private RoadSegment BuildCollectedRoadSegment(RoadSegmentId roadSegmentId, Coordinate from, Coordinate to, RoadNodeId startNodeId, RoadNodeId endNodeId)
    {
        var geometry = Line(from, to);

        return RoadSegment.Create(TestData.Segment1Added with
        {
            RoadSegmentId = roadSegmentId,
            Geometry = geometry.ToRoadSegmentGeometry(),
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
        }).WithoutChanges();
    }

    private static RoadNode BuildRemovedRoadNode(RoadNodeId roadNodeId, Coordinate coordinate)
    {
        var removed = RoadNode.CreateForMigration(roadNodeId, Node(coordinate));
        removed.Remove(new FakeProvenance());

        return removed.WithoutChanges();
    }

    private RoadSegmentDynamicAttributeValues<T> Attribute<T>(MultiLineString geometry)
    {
        return new RoadSegmentDynamicAttributeValues<T>()
            .Add(Fixture.Create<T>(), RoadSegmentGeometry.Create(geometry));
    }

    private static MultiLineString Line(Coordinate from, Coordinate to)
    {
        return new MultiLineString([new LineString([from, to])])
        {
            SRID = WellknownSrids.Lambert08
        };
    }

    private static RoadNodeGeometry Node(Coordinate coordinate)
    {
        return new Point(coordinate) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry();
    }

    private static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 1000);
    }
}
