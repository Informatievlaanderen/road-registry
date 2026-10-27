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

// GAWR-7826, the shape the second of two adjacent inwinning tiles actually arrives in. Its uploader collected the
// three roads it has as one road, and the feature compare hands that over as a road drawn anew with the three it
// replaces removed - not as a road segment it modifies.
//
//                  a (50,25)   cut-a (100,50)
//                     x------------o
//        ws-100 ...../              |
//   junction  o                     |  ws-300, the stretch neither tile collected yet
//        ws-200 .....\              |
//                     x------------o
//                  b (50,-25)  cut-b (100,-50)
//
// The neighbouring tile collected ws-100 and ws-200 all the way out to the two cuts, and merging the stretches
// before them did away with the road nodes a and b that sat between. This tile still knows those two road nodes
// and draws one road from a, around by way of the two cuts, back to b.
//
// Both ends of that road run into what the neighbour already collected, so both are cut back, and what is left is
// the stretch between the two cuts - the only one this tile still had to collect. The roads it says to remove
// along with it are the neighbour's, and stay.
public class NeighbourMergeDrawnAnewTests : RoadNetworkTestBase
{
    private static readonly Coordinate Junction = new(0, 0);
    private static readonly Coordinate A = new(50, 25);
    private static readonly Coordinate B = new(50, -25);
    private static readonly Coordinate CutA = new(100, 50);
    private static readonly Coordinate CutB = new(100, -50);

    private static readonly RoadNodeId JunctionNodeId = new(1);
    private static readonly RoadNodeId ANodeId = new(2);
    private static readonly RoadNodeId BNodeId = new(3);
    private static readonly RoadNodeId CutANodeId = new(4);
    private static readonly RoadNodeId CutBNodeId = new(5);

    private static readonly RoadSegmentId LeftSegmentId = new(100);
    private static readonly RoadSegmentId RightSegmentId = new(200);
    private static readonly RoadSegmentId BetweenSegmentId = new(300);

    private static MultiLineString LeftRoad => Line(Junction, A, CutA);
    private static MultiLineString RightRoad => Line(Junction, B, CutB);
    private static MultiLineString RoadBetweenTheCuts => Line(CutA, CutB);

    [Fact]
    public void WhenTheRoadIsDrawnAnew_ThenBothRoadNodesAreAccountedFor()
    {
        var roadNetwork = BuildNetworkWithTheNeighbourTileCollected();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().BeEmpty();
    }

    [Fact]
    public void WhenTheRoadIsDrawnAnew_ThenWhatTheNeighbourCollectedIsNotRemoved()
    {
        var roadNetwork = BuildNetworkWithTheNeighbourTileCollected();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());
        result.Problems.Select(x => x.Reason).Should().BeEmpty();

        // This upload says to remove both of them, believing the road it draws replaces them. The cuts handed
        // those stretches back, so they are the neighbour's and not this upload's to remove.
        roadNetwork.RoadSegments[LeftSegmentId].IsRemoved.Should().BeFalse();
        roadNetwork.RoadSegments[RightSegmentId].IsRemoved.Should().BeFalse();

        // The one stretch this tile still had to collect is the one it replaces.
        roadNetwork.RoadSegments[BetweenSegmentId].IsRemoved.Should().BeTrue();
    }

    [Fact]
    public void WhenTheRoadIsDrawnAnew_ThenOnlyTheStretchBetweenTheCutsIsCollected()
    {
        var roadNetwork = BuildNetworkWithTheNeighbourTileCollected();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());
        result.Problems.Select(x => x.Reason).Should().BeEmpty();

        var remaining = roadNetwork.RoadSegments.Values
            .Where(x => !x.IsRemoved && x.Status == RoadSegmentStatusV2.Gerealiseerd)
            .ToArray();

        // Nothing is collected twice and nothing is lost: what is on record is the two roads the neighbour
        // collected plus the stretch between the cuts, however the road network ends up dividing them.
        remaining.Sum(x => x.Geometry.Value.Length).Should().BeApproximately(
            LeftRoad.Length + RightRoad.Length + RoadBetweenTheCuts.Length, 0.01);
    }

    // The network as this tile finds it: the neighbouring tile's two roads collected out to the cuts, the two road
    // nodes its merges did away with still on record as removed, and the stretch between the cuts untouched.
    private ScopedRoadNetwork BuildNetworkWithTheNeighbourTileCollected()
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes:
            [
                RoadNode.CreateForMigration(JunctionNodeId, Node(Junction)),
                RoadNode.CreateForMigration(CutANodeId, Node(CutA)),
                RoadNode.CreateForMigration(CutBNodeId, Node(CutB)),
                BuildRemovedRoadNode(ANodeId, A),
                BuildRemovedRoadNode(BNodeId, B)
            ],
            roadSegments:
            [
                BuildCollectedRoadSegment(LeftSegmentId, LeftRoad, JunctionNodeId, CutANodeId),
                BuildCollectedRoadSegment(RightSegmentId, RightRoad, JunctionNodeId, CutBNodeId),
                RoadSegment.CreateForMigration(BetweenSegmentId, RoadBetweenTheCuts.ToRoadSegmentGeometry(), RoadSegmentStatusV2.Gerealiseerd, CutANodeId, CutBNodeId)
            ]).WithoutChanges();
    }

    // What this tile uploads: one road drawn from the one road node its neighbour merged away, by way of the two
    // cuts, to the other, with the three roads it stands in for removed.
    private RoadNetworkChanges Upload()
    {
        var geometry = Line(A, CutA, CutB, B);

        var changes = RoadNetworkChanges.Start()
            .WithProvenance(new FakeProvenance())
            .Add(new AddRoadSegmentChange
            {
                RoadSegmentIdReference = new RoadSegmentIdReference(new RoadSegmentId(1)),
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

        foreach (var (roadNodeId, coordinate) in new[] { (ANodeId, A), (BNodeId, B) })
        {
            changes = changes.Add(new ModifyRoadNodeChange
            {
                RoadNodeId = roadNodeId,
                Geometry = Node(coordinate),
                Grensknoop = false
            });
        }

        foreach (var roadNodeId in new[] { CutANodeId, CutBNodeId })
        {
            changes = changes.Add(new RemoveRoadNodeChange { RoadNodeId = roadNodeId });
        }

        foreach (var roadSegmentId in new[] { LeftSegmentId, BetweenSegmentId, RightSegmentId })
        {
            changes = changes.Add(new RemoveRoadSegmentChange { RoadSegmentId = roadSegmentId });
        }

        return changes;
    }

    private RoadSegment BuildCollectedRoadSegment(RoadSegmentId roadSegmentId, MultiLineString geometry, RoadNodeId startNodeId, RoadNodeId endNodeId)
    {
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

    private static MultiLineString Line(params Coordinate[] coordinates)
    {
        return new MultiLineString([new LineString(coordinates)])
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
