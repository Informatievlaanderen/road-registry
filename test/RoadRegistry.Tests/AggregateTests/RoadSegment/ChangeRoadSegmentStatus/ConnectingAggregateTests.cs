namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ChangeRoadSegmentStatus;

using System.Linq;
using FluentAssertions;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Extensions;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

// The status changes that hook a road segment into the network: everything that ends up 'gerealiseerd'.
public class ConnectingAggregateTests : StatusChangeAggregateTestBase
{
    // An existing realized road, running west to east along y = 0, split at (100, 0) so there is a node to connect to.
    private const int RealizedSegmentId = NeighbourSegmentId;
    private const int SecondRealizedSegmentId = SecondNeighbourSegmentId;

    // Every transition into 'gerealiseerd'. The name is what the transition is called on the queue, so it doubles as
    // the InlineData constant.
    public const string PlannedToRealized = "ChangeRoadSegmentFromPlannedToRealized";
    public const string OutOfUseToRealized = "ChangeRoadSegmentFromOutOfUseToRealized";
    public const string HistorizedToRealized = "CorrectRoadSegmentFromHistorizedToRealized";

    // An existing realized road from (0,0) to (200,0), already split at (100,0): the middle node is the one the
    // segment can hook onto. The segment being changed runs north from there, its end point given by the caller.
    private ScopedRoadNetwork BuildNetworkWith(
        RoadSegmentGeometry geometry,
        RoadSegmentStatusV2? status = null,
        RoadSegmentGeometryDrawMethodV2? drawMethod = null)
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var eastNode = BuildNode(12, 200, 0, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [westNode, middleNode, eastNode],
            [
                BuildSegment(ChangedSegmentId, null, null, geometry, status ?? RoadSegmentStatusV2.Gepland, drawMethod),
                BuildSegment(RealizedSegmentId, westNode, middleNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(SecondRealizedSegmentId, middleNode, eastNode, BuildGeometry((100, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd)
            ]);
    }

    [Theory]
    [InlineData(PlannedToRealized)]
    [InlineData(OutOfUseToRealized)]
    [InlineData(HistorizedToRealized)]
    public void WhenTheSegmentIsConnected_ThenItBecomesRealizedAndHangsOffRoadNodes(string change)
    {
        var statusChange = RoadSegmentStatusChange.Parse(change);
        // Drawn exactly on the existing node at (100,0), so nothing has to move.
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (100, 80)), statusChange.From);

        var result = Act(roadNetwork, statusChange);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        roadSegment.Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);
        roadSegment.StartNodeId.Should().Be(new RoadNodeId(11));

        // The far end found nothing, so it got an end node of its own.
        result.Summary.RoadNodes.Added.Should().ContainSingle();
        roadSegment.EndNodeId.Should().Be(result.Summary.RoadNodes.Added.Single());
    }

    [Theory]
    [InlineData(PlannedToRealized)]
    [InlineData(OutOfUseToRealized)]
    [InlineData(HistorizedToRealized)]
    public void WhenTheSegmentIsConnected_ThenTheEventOfThatTransitionIsRecorded(string change)
    {
        // Connecting is its own action, not a modification, and the event records what the segment became in full.
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildNetworkWith(BuildGeometry((100.4, 0), (100.4, 80)), statusChange.From);

        var result = Act(roadNetwork, statusChange);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        roadSegment.GetChanges().OfType<RoadSegmentWasModified>().Should().BeEmpty();

        var @event = roadSegment.GetChanges().OfType<IRoadSegmentWasConnectedEvent>().Should().ContainSingle().Which;
        @event.Should().BeOfType(statusChange.EventType);
        @event.RoadSegmentId.Should().Be(new RoadSegmentId(ChangedSegmentId));
        @event.StartNodeId.Should().Be(new RoadNodeId(11));
        @event.EndNodeId.Should().Be(roadSegment.EndNodeId!.Value);

        // The geometry it carries is the snapped one, not the one as drawn.
        @event.Geometry.Value.GetSingleLineString().Coordinates[0].X.Should().BeApproximately(100, 0.001);

        // Every dynamically segmented attribute is there, remapped onto that geometry.
        var length = @event.Geometry.Value.Length;
        @event.AccessRestriction.Values.Should().NotBeEmpty();
        @event.Category.Values.Should().NotBeEmpty();
        @event.Morphology.Values.Should().NotBeEmpty();
        @event.StreetNameId.Values.Should().NotBeEmpty();
        @event.MaintenanceAuthorityId.Values.Should().NotBeEmpty();
        @event.SurfaceType.Values.Should().NotBeEmpty();
        @event.CarTrafficDirection.Values.Should().NotBeEmpty();
        @event.BikeTrafficDirection.Values.Should().NotBeEmpty();
        @event.PedestrianTrafficDirection.Values.Should().NotBeEmpty();
        @event.Morphology.Values.Max(x => x.Coverage.To.ToDouble()).Should().BeApproximately(length, 0.01);
    }

    [Theory]
    [InlineData(PlannedToRealized, nameof(RoadSegmentStatusV2.Gerealiseerd))]
    [InlineData(PlannedToRealized, nameof(RoadSegmentStatusV2.BuitenGebruik))]
    [InlineData(OutOfUseToRealized, nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(OutOfUseToRealized, nameof(RoadSegmentStatusV2.Gerealiseerd))]
    [InlineData(HistorizedToRealized, nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(HistorizedToRealized, nameof(RoadSegmentStatusV2.BuitenGebruik))]
    public void WhenTheStatusIsNotTheOneBeingChangedAwayFrom_ThenError(string change, string status)
    {
        // VAL-4
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Parse(status));

        var result = Act(roadNetwork, statusChange);

        result.Problems.Select(x => x.Reason).Should().Contain(statusChange.StatusNotValidProblemCode.ToString());
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Parse(status));
    }

    [Fact]
    public void WhenTheStartPointIsNearAnExistingNode_ThenTheGeometryIsSnappedOntoIt()
    {
        // 40cm off the node at (100,0), which is within the one metre reach.
        var roadNetwork = BuildNetworkWith(BuildGeometry((100.4, 0), (100.4, 80)));

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        roadSegment.StartNodeId.Should().Be(new RoadNodeId(11));

        // Only the first vertex moved; the one after it stayed exactly where it was drawn.
        var coordinates = roadSegment.Geometry.Value.GetSingleLineString().Coordinates;
        coordinates[0].X.Should().BeApproximately(100, 0.001);
        coordinates[0].Y.Should().BeApproximately(0, 0.001);
        coordinates[^1].X.Should().BeApproximately(100.4, 0.001);
    }

    [Fact]
    public void WhenAnExistingNodeIsJustOutOfReach_ThenAnEndNodeIsAddedInstead()
    {
        // 1.4m away from the node at (100,0): too far to snap onto, so this end terminates on a node of its own. The
        // other end is drawn on the node so the segment still connects to the network.
        var roadNetwork = BuildNetworkWith(BuildGeometry((101.4, 80), (100, 0)));

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        roadSegment.EndNodeId.Should().Be(new RoadNodeId(11));

        var addedNodeId = result.Summary.RoadNodes.Added.Should().ContainSingle().Which;
        roadSegment.StartNodeId.Should().Be(addedNodeId);

        // Placed where the segment was drawn, not pulled towards the node it could not reach.
        roadNetwork.RoadNodes[addedNodeId].Geometry.Value.Coordinate.X.Should().BeApproximately(101.4, 0.001);
    }

    [Fact]
    public void WhenTwoNodesAreInReach_ThenItSnapsOntoTheNearest()
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var eastNode = BuildNode(12, 200, 0, RoadNodeTypeV2.Eindknoop);
        // A stub hanging south, so there is a second road node within reach of the start point.
        var stubNode = BuildNode(13, 100.9, 0, RoadNodeTypeV2.Eindknoop);
        var stubEndNode = BuildNode(14, 100.9, -60, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [westNode, middleNode, eastNode, stubNode, stubEndNode],
            [
                // 30cm to the middle node against 60cm to the stub node.
                BuildSegment(ChangedSegmentId, null, null, BuildGeometry((100.3, 0), (100.3, 80)), RoadSegmentStatusV2.Gepland),
                BuildSegment(RealizedSegmentId, westNode, middleNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(SecondRealizedSegmentId, middleNode, eastNode, BuildGeometry((100, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(4, stubNode, stubEndNode, BuildGeometry((100.9, 0), (100.9, -60)), RoadSegmentStatusV2.Gerealiseerd)
            ]);

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].StartNodeId.Should().Be(new RoadNodeId(11));
    }

    [Fact]
    public void WhenNeitherEndPointHasANodeInReach_ThenError()
    {
        // VAL-5: an island. Far away from the existing road at y = 0.
        var roadNetwork = BuildNetworkWith(BuildGeometry((500, 500), (500, 580)));

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized);

        result.Problems.Select(x => x.Reason).Should().Contain(RoadSegmentRealizeNoRoadNodeInReach.ProblemCode.ToString());
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gepland);
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayNotTouchThose_ThenError()
    {
        // VAL-9
        var roadNetwork = BuildNetworkWith(
            BuildGeometry((100, 0), (100, 80)),
            drawMethod: RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized, mayModifyMeasuredRoadSegments: false);

        result.Problems.Select(x => x.Reason).Should().Contain(RoadSegmentMeasuredNotAllowed.ProblemCode.ToString());
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gepland);
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayTouchThose_ThenItIsRealized()
    {
        var roadNetwork = BuildNetworkWith(
            BuildGeometry((100, 0), (100, 80)),
            drawMethod: RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized, mayModifyMeasuredRoadSegments: true);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);
    }

    [Fact]
    public void WhenTheSegmentDoesNotExist_ThenNotFound()
    {
        // VAL-2
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (100, 80)));

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized, roadSegmentId: 999);

        result.Problems.Select(x => x.Reason).Should().Contain(ProblemCode.RoadSegment.NotFound.ToString());
    }

    [Fact]
    public void WhenTheSegmentCrossesARealizedSegment_ThenAGradeJunctionIsAdded()
    {
        // Hooks onto the node at (100,0) and runs back west across the realized road at y = 0.
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (50, 40), (20, -40)));

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeJunctions.Added.Should().ContainSingle();
    }

    [Fact]
    public void WhenTheSegmentHasTheSameStartAndEndPoint_ThenError()
    {
        // VAL-7
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (140, 40), (60, 40), (100, 0)));

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized);

        result.Problems.Should().NotBeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gepland);
    }

    [Fact]
    public void WhenTheSegmentCrossesItself_ThenError()
    {
        // VAL-8
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (100, 80), (60, 40), (140, 40)));

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized);

        result.Problems.Should().NotBeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gepland);
    }

    [Fact]
    public void WhenItSnapsOntoAnEindknoop_ThenTheRoadItHooksOntoIsNotMergedAway()
    {
        // The node at (100,0) terminates an existing road. Hooking on leaves it with two segments, which the
        // network-wide rules would take as licence to merge the two into one. An edit of one segment may not do that.
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var deadEndNode = BuildNode(11, 100, 0, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [westNode, deadEndNode],
            [
                BuildSegment(ChangedSegmentId, null, null, BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Gepland),
                BuildSegment(RealizedSegmentId, westNode, deadEndNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd)
            ]);

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized);

        result.Problems.Should().BeEmpty();

        // Both segments are still there under their own identifiers, and neither was removed in favour of a merge.
        result.Summary.RoadSegments.Removed.Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].IsRemoved.Should().BeFalse();
        roadNetwork.RoadSegments[new RoadSegmentId(RealizedSegmentId)].IsRemoved.Should().BeFalse();

        // And the segment really did hook onto that node.
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].StartNodeId.Should().Be(new RoadNodeId(11));
    }

    [Fact]
    public void WhenTheSegmentIsConnected_ThenTheNodeItConnectedToIsRetyped()
    {
        // The node at (100,0) had two segments and was a 'validatieknoop'; with a third it is an 'echte knoop'.
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (100, 80)));

        var result = Act(roadNetwork, RoadSegmentStatusChange.PlannedToRealized);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadNodes[new RoadNodeId(11)].Type.Should().Be(RoadNodeTypeV2.EchteKnoop);
    }
}
