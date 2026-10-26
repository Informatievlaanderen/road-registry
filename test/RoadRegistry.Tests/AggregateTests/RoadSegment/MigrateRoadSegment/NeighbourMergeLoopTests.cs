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

// GAWR-7826. The roads here run in a loop that comes back to the same place, and the border between the two
// municipalities cuts straight through it. The neighbour collected the bottom of the loop as two roads that meet at
// the junction, and in doing so merged away the road node at each of the two places the border crosses.
//
// This upload brings the top of the loop: one road that runs from the one border crossing, up and over, down to the
// other. Both of its ends run into a stretch its neighbour has already collected - two different roads, one at each
// end - so both ends have to be cut back, and both road nodes are accounted for by that.
//
//      cut-a (0,110)  o-------------------o  cut-b (200,110)      the road nodes the cuts land on
//                     |   ws-300 (upload) |
//        a (0,50)     x                   x  b (200,50)           merged away by the neighbour
//             \ ws-100                   / ws-200
//              \                        /
//               o----------------------o
//                   junction (100,0)
//
// What is left of the upload is the top of the loop between the two cuts. It joins what the neighbour collected at
// one of the two cuts; joining at both would close the loop into a road that starts and ends at the junction, which
// the register cannot record as one road.
public class NeighbourMergeLoopTests : RoadNetworkTestBase
{
    private static readonly Coordinate Junction = new(100, 0);
    private static readonly Coordinate A = new(0, 50);
    private static readonly Coordinate B = new(200, 50);
    private static readonly Coordinate CutA = new(0, 110);
    private static readonly Coordinate CutB = new(200, 110);
    private static readonly Coordinate TopLeft = new(0, 160);
    private static readonly Coordinate TopRight = new(200, 160);

    private static readonly RoadNodeId JunctionNodeId = new(1);
    private static readonly RoadNodeId ANodeId = new(2);
    private static readonly RoadNodeId BNodeId = new(3);
    private static readonly RoadNodeId CutANodeId = new(4);
    private static readonly RoadNodeId CutBNodeId = new(5);

    private static readonly RoadSegmentId LeftSegmentId = new(100);
    private static readonly RoadSegmentId RightSegmentId = new(200);
    private static readonly RoadSegmentId UploadedSegmentId = new(300);

    [Fact]
    public void WhenBothEndsRunIntoWhatTheNeighbourCollected_ThenBothAreAccountedFor()
    {
        var roadNetwork = BuildNetworkWithTheBottomOfTheLoopCollected();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().BeEmpty();
    }

    [Fact]
    public void WhenBothEndsRunIntoWhatTheNeighbourCollected_ThenItIsCutBackAtBothEnds()
    {
        var roadNetwork = BuildNetworkWithTheBottomOfTheLoopCollected();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());
        result.Problems.Select(x => x.Reason).Should().BeEmpty();

        // What is left runs between the two cuts and no further: the stretches down to the border crossings were
        // collected by the neighbour already.
        roadNetwork.RoadSegments[UploadedSegmentId].Geometry.Value.Length
            .Should().BeApproximately(TopOfTheLoop.Length, 0.01);
    }

    [Fact]
    public void WhenBothEndsRunIntoWhatTheNeighbourCollected_ThenWhatIsLeftIsJoinedToOneOfThemOnly()
    {
        var roadNetwork = BuildNetworkWithTheBottomOfTheLoopCollected();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());
        result.Problems.Select(x => x.Reason).Should().BeEmpty();

        var remaining = roadNetwork.RoadSegments.Values
            .Where(x => !x.IsRemoved && x.Status == RoadSegmentStatusV2.Gerealiseerd)
            .ToArray();

        // Joined at one of the two cuts and left alone at the other: joining at both would close the loop into a
        // road that begins and ends at the junction, which is not a road the register can record.
        remaining.Should().HaveCount(2);
        remaining.Should().OnlyContain(x => x.StartNodeId != x.EndNodeId);
        remaining.Sum(x => x.Geometry.Value.Length)
            .Should().BeApproximately(TopOfTheLoop.Length + 2 * BottomHalfOfTheLoop.Length, 0.01);
    }

    private static MultiLineString TopOfTheLoop => Line(CutA, TopLeft, TopRight, CutB);
    private static MultiLineString BottomHalfOfTheLoop => Line(Junction, A, CutA);

    // The network as this municipality finds it: the bottom of the loop collected as two roads meeting at the
    // junction, the two border crossings merged away with them, and its own road still untouched.
    private ScopedRoadNetwork BuildNetworkWithTheBottomOfTheLoopCollected()
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
                RoadSegment.CreateForMigration(UploadedSegmentId, Line(A, TopLeft, TopRight, B).ToRoadSegmentGeometry(), RoadSegmentStatusV2.Gerealiseerd, ANodeId, BNodeId),
                BuildCollectedRoadSegment(LeftSegmentId, Line(Junction, A, CutA), JunctionNodeId, CutANodeId),
                BuildCollectedRoadSegment(RightSegmentId, Line(Junction, B, CutB), JunctionNodeId, CutBNodeId)
            ]).WithoutChanges();
    }

    // What the municipality uploads: its own road, the two border crossings its neighbour did away with, and the two
    // road nodes the cuts land on.
    private RoadNetworkChanges Upload()
    {
        var geometry = Line(A, TopLeft, TopRight, B);

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

        foreach (var (roadNodeId, coordinate) in new[] { (ANodeId, A), (BNodeId, B), (CutANodeId, CutA), (CutBNodeId, CutB) })
        {
            changes = changes.Add(new ModifyRoadNodeChange
            {
                RoadNodeId = roadNodeId,
                Geometry = Node(coordinate),
                Grensknoop = false
            });
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
