namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ChangeRoadSegmentStatus;

using System.Linq;
using FluentAssertions;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

// The status changes between two statuses that both leave the segment outside the network. Nothing but the status
// moves: no road nodes, no crossings, no geometry, no attributes.
public class UnconnectedAggregateTests : StatusChangeAggregateTestBase
{
    // The name is what the transition is called on the queue, so it doubles as the InlineData constant.
    public const string OutOfUseToHistorized = "ChangeRoadSegmentFromOutOfUseToHistorized";
    public const string NotRealizedToPlanned = "CorrectRoadSegmentFromNotRealizedToPlanned";
    public const string HistorizedToOutOfUse = "CorrectRoadSegmentFromHistorizedToOutOfUse";

    // A segment outside the network, running north from (100,0), alongside a realized road that shares no node with
    // it - it carries no road nodes at all.
    private ScopedRoadNetwork BuildNetworkWith(
        RoadSegmentStatusV2 status,
        RoadSegmentGeometryDrawMethodV2? drawMethod = null)
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(11, 200, 0, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [westNode, eastNode],
            [
                BuildSegment(ChangedSegmentId, null, null, BuildGeometry((100, 40), (100, 120)), status, drawMethod),
                BuildSegment(NeighbourSegmentId, westNode, eastNode, BuildGeometry((0, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd)
            ]);
    }

    [Theory]
    [InlineData(OutOfUseToHistorized)]
    [InlineData(NotRealizedToPlanned)]
    [InlineData(HistorizedToOutOfUse)]
    public void WhenTheStatusIsChanged_ThenNothingButTheStatusMoves(string change)
    {
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildNetworkWith(statusChange.From);
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        var geometryBefore = roadSegment.Geometry;
        var attributesBefore = roadSegment.Attributes;

        var result = Act(roadNetwork, statusChange);

        result.Problems.Should().BeEmpty();
        roadSegment.Status.Should().Be(statusChange.To);
        roadSegment.Geometry.Should().Be(geometryBefore);
        roadSegment.Attributes.Should().Be(attributesBefore);
        roadSegment.StartNodeId.Should().BeNull();
        roadSegment.EndNodeId.Should().BeNull();
    }

    [Theory]
    [InlineData(OutOfUseToHistorized)]
    [InlineData(NotRealizedToPlanned)]
    [InlineData(HistorizedToOutOfUse)]
    public void WhenTheStatusIsChanged_ThenTheEventOfThatTransitionIsRecorded(string change)
    {
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildNetworkWith(statusChange.From);

        var result = Act(roadNetwork, statusChange);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        roadSegment.GetChanges().OfType<RoadSegmentWasModified>().Should().BeEmpty();

        var @event = roadSegment.GetChanges().OfType<IRoadSegmentUnconnectedStatusChangeEvent>().Should().ContainSingle().Which;
        @event.Should().BeOfType(statusChange.EventType);
        @event.RoadSegmentId.Should().Be(new RoadSegmentId(ChangedSegmentId));
    }

    [Theory]
    [InlineData(OutOfUseToHistorized)]
    [InlineData(NotRealizedToPlanned)]
    [InlineData(HistorizedToOutOfUse)]
    public void WhenTheStatusIsChanged_ThenTheSurroundingNetworkIsLeftAlone(string change)
    {
        // The segment is not part of the network before or after, so there is no topology to work out at all.
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildNetworkWith(statusChange.From);

        var result = Act(roadNetwork, statusChange);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Modified.Should().ContainSingle().Which.Should().Be(new RoadSegmentId(ChangedSegmentId));
        result.Summary.RoadSegments.Removed.Should().BeEmpty();
        result.Summary.RoadNodes.Added.Should().BeEmpty();
        result.Summary.RoadNodes.Modified.Should().BeEmpty();
        result.Summary.RoadNodes.Removed.Should().BeEmpty();
        result.Summary.GradeJunctions.Added.Should().BeEmpty();
        result.Summary.GradeJunctions.Removed.Should().BeEmpty();
        result.Summary.GradeSeparatedJunctions.Removed.Should().BeEmpty();
    }

    // Already there: the change has nothing left to do, so it is answered as the success it is rather than as a
    // problem about the status the segment is not changing away from.
    [Theory]
    [InlineData(OutOfUseToHistorized)]
    [InlineData(NotRealizedToPlanned)]
    [InlineData(HistorizedToOutOfUse)]
    public void WhenTheSegmentAlreadyHasTheStatusItIsChangedTo_ThenNothingHappens(string change)
    {
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildNetworkWith(statusChange.To);
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];

        var result = Act(roadNetwork, statusChange);

        result.Problems.Should().BeEmpty();
        result.Summary.HasChanges().Should().BeFalse();
        roadSegment.GetChanges().Should().BeEmpty();
        roadSegment.Status.Should().Be(statusChange.To);
    }

    [Theory]
    [InlineData(OutOfUseToHistorized, nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(OutOfUseToHistorized, nameof(RoadSegmentStatusV2.Gerealiseerd))]
    [InlineData(NotRealizedToPlanned, nameof(RoadSegmentStatusV2.BuitenGebruik))]
    [InlineData(HistorizedToOutOfUse, nameof(RoadSegmentStatusV2.Gerealiseerd))]
    public void WhenTheStatusIsNotTheOneBeingChangedAwayFrom_ThenError(string change, string status)
    {
        // VAL-4
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Parse(status));

        var result = Act(roadNetwork, statusChange);

        result.Problems.Select(x => x.Reason).Should().Contain(statusChange.StatusNotValidProblemCode.ToString());
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Parse(status));
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayNotTouchThose_ThenError()
    {
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.BuitenGebruik, RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = Act(roadNetwork, RoadSegmentStatusChange.OutOfUseToHistorized, mayModifyMeasuredRoadSegments: false);

        result.Problems.Select(x => x.Reason).Should().Contain(RoadSegmentMeasuredNotAllowed.ProblemCode.ToString());
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.BuitenGebruik);
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayTouchThose_ThenItIsChanged()
    {
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.BuitenGebruik, RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = Act(roadNetwork, RoadSegmentStatusChange.OutOfUseToHistorized, mayModifyMeasuredRoadSegments: true);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);
    }

    [Fact]
    public void WhenTheSegmentDoesNotExist_ThenNotFound()
    {
        // VAL-2
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.BuitenGebruik);

        var result = Act(roadNetwork, RoadSegmentStatusChange.OutOfUseToHistorized, roadSegmentId: 999);

        result.Problems.Select(x => x.Reason).Should().Contain(ProblemCode.RoadSegment.NotFound.ToString());
    }
}
