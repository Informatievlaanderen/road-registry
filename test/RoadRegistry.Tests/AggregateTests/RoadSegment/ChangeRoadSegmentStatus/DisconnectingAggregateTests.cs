namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ChangeRoadSegmentStatus;

using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

// The status changes that unhook a road segment from the network: everything that leaves 'gerealiseerd'.
public class DisconnectingAggregateTests : StatusChangeAggregateTestBase
{
    // Every transition out of 'gerealiseerd'. The name is what the transition is called on the queue, so it doubles as
    // the InlineData constant.
    public const string RealizedToPlanned = "CorrectRoadSegmentFromRealizedToPlanned";
    public const string RealizedToOutOfUse = "ChangeRoadSegmentFromRealizedToOutOfUse";
    public const string RealizedToHistorized = "ChangeRoadSegmentFromRealizedToHistorized";

    // A realized segment running north from (100,0), on its own: both its road nodes carry nothing else.
    private ScopedRoadNetwork BuildNetworkWithASegmentOnItsOwn(
        RoadSegmentStatusV2? status = null,
        RoadSegmentGeometryDrawMethodV2? drawMethod = null)
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [southNode, northNode],
            [BuildSegment(ChangedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)), status ?? RoadSegmentStatusV2.Gerealiseerd, drawMethod)]);
    }

    [Theory]
    [InlineData(RealizedToPlanned)]
    [InlineData(RealizedToOutOfUse)]
    [InlineData(RealizedToHistorized)]
    public void WhenTheSegmentIsDisconnected_ThenItTakesTheNewStatusAndGivesUpItsRoadNodes(string change)
    {
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn();

        var result = Act(roadNetwork, statusChange);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        roadSegment.Status.Should().Be(statusChange.To);
        roadSegment.StartNodeId.Should().BeNull();
        roadSegment.EndNodeId.Should().BeNull();
    }

    [Theory]
    [InlineData(RealizedToPlanned)]
    [InlineData(RealizedToOutOfUse)]
    [InlineData(RealizedToHistorized)]
    public void WhenTheSegmentIsDisconnected_ThenTheEventOfThatTransitionIsRecorded(string change)
    {
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn();

        var result = Act(roadNetwork, statusChange);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        roadSegment.GetChanges().OfType<RoadSegmentWasModified>().Should().BeEmpty();

        // The nodes it came loose from are named, so a reader can tell what it was hooked onto.
        var @event = roadSegment.GetChanges().OfType<IRoadSegmentWasDisconnectedEvent>().Should().ContainSingle().Which;
        @event.Should().BeOfType(statusChange.EventType);
        @event.RoadSegmentId.Should().Be(new RoadSegmentId(ChangedSegmentId));
        @event.PreviousStartNodeId.Should().Be(new RoadNodeId(10));
        @event.PreviousEndNodeId.Should().Be(new RoadNodeId(11));
    }

    [Theory]
    [InlineData(RealizedToPlanned)]
    [InlineData(RealizedToOutOfUse)]
    [InlineData(RealizedToHistorized)]
    public void WhenARoadNodeIsLeftCarryingNothing_ThenItIsRemoved(string change)
    {
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn();

        var result = Act(roadNetwork, RoadSegmentStatusChange.Parse(change));

        result.Problems.Should().BeEmpty();
        result.Summary.RoadNodes.Removed.Should().BeEquivalentTo([new RoadNodeId(10), new RoadNodeId(11)]);
        roadNetwork.RoadNodes[new RoadNodeId(10)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadNodes[new RoadNodeId(11)].IsRemoved.Should().BeTrue();
    }

    [Theory]
    [InlineData(RealizedToPlanned, nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(RealizedToPlanned, nameof(RoadSegmentStatusV2.BuitenGebruik))]
    [InlineData(RealizedToOutOfUse, nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(RealizedToHistorized, nameof(RoadSegmentStatusV2.BuitenGebruik))]
    public void WhenTheStatusIsNotGerealiseerd_ThenError(string change, string status)
    {
        // VAL-4
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn(RoadSegmentStatusV2.Parse(status));

        var result = Act(roadNetwork, statusChange);

        result.Problems.Select(x => x.Reason).Should().Contain(statusChange.StatusNotValidProblemCode.ToString());
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Parse(status));
    }

    [Fact]
    public void WhenTheGeometryAndAttributesAreUntouched_ThenTheyStayExactlyAsTheyWere()
    {
        // The segment is still drawn where it was drawn; it simply is not part of the network any more.
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        var geometryBefore = roadSegment.Geometry;
        var attributesBefore = roadSegment.Attributes;

        var result = Act(roadNetwork, RoadSegmentStatusChange.RealizedToPlanned);

        result.Problems.Should().BeEmpty();
        roadSegment.Geometry.Should().Be(geometryBefore);
        roadSegment.Attributes.Should().Be(attributesBefore);
    }

    [Fact]
    public void WhenARoadNodeStillCarriesAnotherSegment_ThenItSurvivesAndIsRetyped()
    {
        // The shared node at (100,0) carries three segments while this one is realized, so it is an 'echte knoop'.
        // Once this one comes loose two are left, which makes it a 'validatieknoop'.
        var sharedNode = BuildNode(10, 100, 0, RoadNodeTypeV2.EchteKnoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(12, 0, 0, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 0, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [sharedNode, northNode, westNode, eastNode],
            [
                BuildSegment(ChangedSegmentId, sharedNode, northNode, BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(NeighbourSegmentId, westNode, sharedNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(SecondNeighbourSegmentId, sharedNode, eastNode, BuildGeometry((100, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd)
            ]);

        var result = Act(roadNetwork, RoadSegmentStatusChange.RealizedToPlanned);

        result.Problems.Should().BeEmpty();

        roadNetwork.RoadNodes[new RoadNodeId(10)].IsRemoved.Should().BeFalse();
        roadNetwork.RoadNodes[new RoadNodeId(10)].Type.Should().Be(RoadNodeTypeV2.Validatieknoop);

        // The far end carried nothing else, so that one does go.
        roadNetwork.RoadNodes[new RoadNodeId(11)].IsRemoved.Should().BeTrue();
    }

    [Fact]
    public void WhenTheNeighboursAreLeftMeetingAtTheNode_ThenTheyAreNotMergedAway()
    {
        // Two realized roads meeting at the node this segment lets go of. The network-wide rules would take that as
        // licence to merge them into one; an editing action names one segment and leaves the others alone.
        var sharedNode = BuildNode(10, 100, 0, RoadNodeTypeV2.EchteKnoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(12, 0, 0, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 0, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [sharedNode, northNode, westNode, eastNode],
            [
                BuildSegment(ChangedSegmentId, sharedNode, northNode, BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(NeighbourSegmentId, westNode, sharedNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(SecondNeighbourSegmentId, sharedNode, eastNode, BuildGeometry((100, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd)
            ]);

        var result = Act(roadNetwork, RoadSegmentStatusChange.RealizedToPlanned);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Removed.Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(NeighbourSegmentId)].IsRemoved.Should().BeFalse();
        roadNetwork.RoadSegments[new RoadSegmentId(SecondNeighbourSegmentId)].IsRemoved.Should().BeFalse();
    }

    [Fact]
    public void WhenTheSegmentTakesPartInAGradeJunction_ThenTheJunctionGoesWithIt()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(12, 0, 40, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 40, RoadNodeTypeV2.Eindknoop);

        var gradeJunction = new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(ChangedSegmentId),
            RoadSegmentId2 = new RoadSegmentId(NeighbourSegmentId),
            Geometry = JunctionGeometry.Create(new Point(new Coordinate(100, 40)) { SRID = WellknownSrids.Lambert08 }),
            Provenance = new ProvenanceData(TestData.Provenance)
        };

        var roadNetwork = BuildNetwork(
            [southNode, northNode, westNode, eastNode],
            [
                BuildSegment(ChangedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(NeighbourSegmentId, westNode, eastNode, BuildGeometry((0, 40), (200, 40)), RoadSegmentStatusV2.Gerealiseerd)
            ],
            gradeJunctions: [gradeJunction]);

        var result = Act(roadNetwork, RoadSegmentStatusChange.RealizedToPlanned);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeJunctions.Removed.Should().ContainSingle()
            .Which.Should().Be(new GradeJunctionId(1));
        roadNetwork.GradeJunctions[new GradeJunctionId(1)].IsRemoved.Should().BeTrue();
    }

    [Fact]
    public void WhenTheSegmentTakesPartInAGradeSeparatedJunction_ThenTheJunctionGoesWithIt()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(12, 0, 40, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 40, RoadNodeTypeV2.Eindknoop);

        var junction = new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            UpperRoadSegmentId = new RoadSegmentId(ChangedSegmentId),
            LowerRoadSegmentId = new RoadSegmentId(NeighbourSegmentId),
            Type = GradeSeparatedJunctionTypeV2.NietGekend,
            Geometry = JunctionGeometry.Create(new Point(new Coordinate(100, 40)) { SRID = WellknownSrids.Lambert08 }),
            Provenance = new ProvenanceData(TestData.Provenance)
        };

        var roadNetwork = BuildNetwork(
            [southNode, northNode, westNode, eastNode],
            [
                BuildSegment(ChangedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(NeighbourSegmentId, westNode, eastNode, BuildGeometry((0, 40), (200, 40)), RoadSegmentStatusV2.Gerealiseerd)
            ],
            gradeSeparatedJunctions: [junction]);

        var result = Act(roadNetwork, RoadSegmentStatusChange.RealizedToPlanned);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeSeparatedJunctions.Removed.Should().ContainSingle()
            .Which.Should().Be(new GradeSeparatedJunctionId(1));
        roadNetwork.GradeSeparatedJunctions[new GradeSeparatedJunctionId(1)].IsRemoved.Should().BeTrue();
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayNotTouchThose_ThenError()
    {
        // VAL-5
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn(drawMethod: RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = Act(roadNetwork, RoadSegmentStatusChange.RealizedToPlanned, mayModifyMeasuredRoadSegments: false);

        result.Problems.Select(x => x.Reason).Should().Contain(RoadSegmentMeasuredNotAllowed.ProblemCode.ToString());
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayTouchThose_ThenItIsChanged()
    {
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn(drawMethod: RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = Act(roadNetwork, RoadSegmentStatusChange.RealizedToPlanned, mayModifyMeasuredRoadSegments: true);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gepland);
    }

    [Fact]
    public void WhenTheSegmentDoesNotExist_ThenNotFound()
    {
        // VAL-2
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn();

        var result = Act(roadNetwork, RoadSegmentStatusChange.RealizedToPlanned, roadSegmentId: 999);

        result.Problems.Select(x => x.Reason).Should().Contain(ProblemCode.RoadSegment.NotFound.ToString());
    }
}
