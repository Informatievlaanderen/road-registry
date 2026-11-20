namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MigrateRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// The road segment's half of the neighbouring-municipality story; see the road node's RemovalCorrectionTests for the
// other. The municipality that uploaded first removed a road segment that crosses the border, along with the road node
// it ran to. The neighbour was prepared against the extract from before that, and still brings that road segment: it
// lies on its own side as well and it is entitled to collect it. The road segment comes back, under the identifier it
// always had, and everything that refers to it by number keeps pointing at it.
//
// One road runs north to south across the border between the two municipalities, with a side road at the border so
// that the road node there is a junction the topology keeps rather than one it merges away:
//
//   Inside (0,100)   o
//                    | ws-200      this municipality's own road, which the neighbour never touched
//   Border (0,0)     x ----- o West (-100,0)
//                    | ws-100      the road across the border, removed along with the road node
//   Across (0,-100)  x             (ws-300 is the side road to the west)
//
// Each test sketches the three states it is about. In those sketches:
//
//   o  a road node                | a road, named ws-<identifier>
//   x  a road node that is removed
//   .  a road that is removed
public class RemovalCorrectionTests : RoadNetworkTestBase
{
    private static readonly Coordinate Inside = new(0, 100);
    private static readonly Coordinate Border = new(0, 0);
    private static readonly Coordinate Across = new(0, -100);
    private static readonly Coordinate West = new(-100, 0);

    private static readonly RoadNodeId InsideNodeId = new(1);
    private static readonly RoadNodeId BorderNodeId = new(2);
    private static readonly RoadNodeId AcrossNodeId = new(3);
    private static readonly RoadNodeId WestNodeId = new(4);

    private static readonly RoadSegmentId AcrossSegmentId = new(100);
    private static readonly RoadSegmentId InsideSegmentId = new(200);
    private static readonly RoadSegmentId WestSegmentId = new(300);

    // The case this is all about: the road segment is brought back and keeps its identifier.
    //
    //   in the network            the upload                result
    //   x border                  o border                  o border
    //   . ws-100                  | ws-100                  | ws-100
    //   x across                  o across                  o across
    [Fact]
    public void WhenTheNeighbourRemovedTheRoad_ThenItIsCollectedAgainUnderTheSameId()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Should().BeEmpty();

        var acrossSegment = roadNetwork.RoadSegments[AcrossSegmentId];
        acrossSegment.RoadSegmentId.Should().Be(AcrossSegmentId);
        acrossSegment.IsRemoved.Should().BeFalse();
        acrossSegment.IsRemovedBecauseOfMigration.Should().BeFalse();
    }

    // The correction says no more than that the removal is undone; the road segment is collected right after it, and
    // that is what carries the state the projections need to build their row again.
    [Fact]
    public void WhenTheNeighbourRemovedTheRoad_ThenItIsCorrectedAndCollectedAgain()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        roadNetwork.Migrate(Upload(), null, IdGenerator());

        var changes = roadNetwork.RoadSegments[AcrossSegmentId].GetChanges();
        changes.Should().SatisfyRespectively(
            first => first.Should().BeOfType<RoadSegmentRemovalWasCorrectedBecauseOfMigration>()
                .Which.RoadSegmentId.Should().Be(AcrossSegmentId),
            second => second.Should().BeOfType<RoadSegmentWasMigrated>()
                .Which.RoadSegmentId.Should().Be(AcrossSegmentId));
    }

    // A road segment that was removed is collected again from scratch rather than modified: that is why the correction
    // drops its attributes. The projections that deleted their row get the geometry, the status, the road nodes and
    // every attribute from a RoadSegmentWasMigrated instead of the partial state a change event carries.
    [Fact]
    public void WhenTheNeighbourRemovedTheRoad_ThenItIsCollectedFromScratchRatherThanModified()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        roadNetwork.Migrate(Upload(), null, IdGenerator());

        var changes = roadNetwork.RoadSegments[AcrossSegmentId].GetChanges();
        changes.Should().Contain(x => x is RoadSegmentWasMigrated);
        changes.Should().NotContain(x => x is RoadSegmentWasChanged);
    }

    // Bringing the road segment back is only worth anything if the rest of the road network sees it again. The spatial
    // indexes are built from what is not removed, so it has to be put back into them by hand.
    [Fact]
    public void WhenTheNeighbourRemovedTheRoad_ThenItIsBackInTheSpatialIndex()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());
        result.Problems.Should().BeEmpty();

        // A road drawn along the middle of the one that was brought back now runs into it, which it could not do
        // while the road segment was missing from the index.
        var overlapping = roadNetwork.ValidatePartiallyOverlappingRoadSegments(
            Line(new Coordinate(0, -80), new Coordinate(0, -20)).ToRoadSegmentGeometry(),
            [],
            new IdentifierTranslator());

        overlapping.Should().NotBeEmpty();
    }

    // The road segment runs to the road node the neighbour removed along with it, and that one is brought back in the
    // same pass - road nodes are handled before the road segments that run to them.
    [Fact]
    public void WhenTheNeighbourRemovedTheRoad_ThenItFindsItsRoadNodesAgain()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());
        result.Problems.Should().BeEmpty();

        var acrossSegment = roadNetwork.RoadSegments[AcrossSegmentId];
        new[] { acrossSegment.StartNodeId, acrossSegment.EndNodeId }.Should().BeEquivalentTo([BorderNodeId, AcrossNodeId]);
    }

    // A road segment removed by an editing action is removed for good: that is not a removal a neighbouring inwinning
    // took from under this upload, so it is not one an inwinning may undo.
    //
    //   in the network            the upload                result
    //   . ws-100                  | ws-100                  . ws-100  <- reported
    [Fact]
    public void WhenTheRoadWasRemovedByAnEditingAction_ThenItIsStillReported()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind(acrossRoadWasRemovedByAnEditingAction: true);

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadSegmentIsRemoved");
        roadNetwork.RoadSegments[AcrossSegmentId].IsRemoved.Should().BeTrue();
    }

    // The road segment is collected from scratch once it is back, and a migration is only one when the change carries
    // the lot. One that does not is not something to bring a road segment back on.
    [Fact]
    public void WhenTheChangeDoesNotCarryEverythingAMigrationNeeds_ThenItIsStillReported()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        var result = roadNetwork.Migrate(UploadWithoutTheStatusOfTheRoadAcross(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadSegmentIsRemoved");
    }

    // Nothing to correct: the road segment the upload names is right where it left it.
    [Fact]
    public void WhenTheRoadIsStillThere_ThenNothingIsCorrected()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(InsideNodeId, Inside), (BorderNodeId, Border), (AcrossNodeId, Across), (WestNodeId, West)],
            roadSegments:
            [
                (InsideSegmentId, Border, Inside, BorderNodeId, InsideNodeId),
                (AcrossSegmentId, Across, Border, AcrossNodeId, BorderNodeId),
                (WestSegmentId, West, Border, WestNodeId, BorderNodeId)
            ]);

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadSegments[AcrossSegmentId].GetChanges().Should()
            .NotContain(x => x is RoadSegmentRemovalWasCorrectedBecauseOfMigration);
    }

    // A road segment the road network never knew is not a removal to correct, however well the upload lines up with it.
    [Fact]
    public void WhenTheRoadWasNeverThere_ThenItIsReportedAsNotFound()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(InsideNodeId, Inside), (BorderNodeId, Border), (AcrossNodeId, Across), (WestNodeId, West)],
            roadSegments:
            [
                (InsideSegmentId, Border, Inside, BorderNodeId, InsideNodeId),
                (WestSegmentId, West, Border, WestNodeId, BorderNodeId)
            ]);

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadSegmentNotFound");
    }

    // The network as the second municipality finds it: its own two roads at the border, and - removed by the
    // neighbour - the road node on the border, the road node across it and the road running between those two.
    private ScopedRoadNetwork BuildNetworkTheNeighbourLeftBehind(bool acrossRoadWasRemovedByAnEditingAction = false)
    {
        return BuildNetwork(
            roadNodes: [(InsideNodeId, Inside), (WestNodeId, West)],
            roadSegments:
            [
                (InsideSegmentId, Border, Inside, BorderNodeId, InsideNodeId),
                (WestSegmentId, West, Border, WestNodeId, BorderNodeId)
            ],
            removedRoadNodes: [(BorderNodeId, Border), (AcrossNodeId, Across)],
            removedRoadSegments: [(AcrossSegmentId, Across, Border, AcrossNodeId, BorderNodeId)],
            removedRoadSegmentsWereRemovedByAnEditingAction: acrossRoadWasRemovedByAnEditingAction);
    }

    // What the municipality uploads: the four road nodes and the three roads between them.
    private RoadNetworkChanges Upload()
    {
        return UploadWithoutTheRoadAcross()
            .Add(RoadSegmentChange(AcrossSegmentId, Across, Border));
    }

    private RoadNetworkChanges UploadWithoutTheStatusOfTheRoadAcross()
    {
        return UploadWithoutTheRoadAcross()
            .Add(RoadSegmentChange(AcrossSegmentId, Across, Border) with { Status = null });
    }

    private RoadNetworkChanges UploadWithoutTheRoadAcross()
    {
        return RoadNetworkChanges.Start()
            .WithProvenance(new FakeProvenance())
            .Add(new ModifyRoadNodeChange { RoadNodeId = InsideNodeId, Geometry = Node(Inside), Grensknoop = false })
            .Add(new ModifyRoadNodeChange { RoadNodeId = BorderNodeId, Geometry = Node(Border), Grensknoop = false })
            .Add(new ModifyRoadNodeChange { RoadNodeId = AcrossNodeId, Geometry = Node(Across), Grensknoop = false })
            .Add(new ModifyRoadNodeChange { RoadNodeId = WestNodeId, Geometry = Node(West), Grensknoop = false })
            .Add(RoadSegmentChange(InsideSegmentId, Border, Inside))
            .Add(RoadSegmentChange(WestSegmentId, West, Border));
    }

    private ModifyRoadSegmentChange RoadSegmentChange(RoadSegmentId roadSegmentId, Coordinate from, Coordinate to)
    {
        var geometry = Line(from, to);

        return new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegmentId),
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
        };
    }

    private ScopedRoadNetwork BuildNetwork(
        (RoadNodeId Id, Coordinate Coordinate)[] roadNodes,
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId)[] roadSegments,
        (RoadNodeId Id, Coordinate Coordinate)[]? removedRoadNodes = null,
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId)[]? removedRoadSegments = null,
        bool removedRoadSegmentsWereRemovedByAnEditingAction = false)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes: roadNodes
                .Select(x => RoadNode.CreateForMigration(x.Id, Node(x.Coordinate)))
                .Concat((removedRoadNodes ?? []).Select(BuildRemovedRoadNode))
                .ToArray(),
            roadSegments: roadSegments
                .Select(BuildRoadSegment)
                .Concat((removedRoadSegments ?? []).Select(x =>
                    BuildRemovedRoadSegment(x, removedRoadSegmentsWereRemovedByAnEditingAction)))
                .ToArray());
    }

    private static RoadSegment BuildRoadSegment(
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId) roadSegment)
    {
        return RoadSegment.CreateForMigration(
            roadSegment.Id,
            Line(roadSegment.From, roadSegment.To).ToRoadSegmentGeometry(),
            RoadSegmentStatusV2.Gerealiseerd,
            roadSegment.StartNodeId,
            roadSegment.EndNodeId);
    }

    // The road network is loaded with what was removed from it still in place, so that is how a road segment a
    // neighbouring inwinning removed arrives here. Which of the two removals it was decides whether it may be
    // corrected: only an inwinning's.
    private static RoadSegment BuildRemovedRoadSegment(
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId) roadSegment,
        bool wasRemovedByAnEditingAction)
    {
        var removed = BuildRoadSegment(roadSegment);

        if (wasRemovedByAnEditingAction)
        {
            removed.Remove(new FakeProvenance());
        }
        else
        {
            removed.RemoveBecauseOfMigration(new FakeProvenance());
        }

        return removed.WithoutChanges();
    }

    private static RoadNode BuildRemovedRoadNode((RoadNodeId Id, Coordinate Coordinate) roadNode)
    {
        var removed = RoadNode.CreateForMigration(roadNode.Id, Node(roadNode.Coordinate));
        removed.RemoveBecauseOfMigration(new FakeProvenance());

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
