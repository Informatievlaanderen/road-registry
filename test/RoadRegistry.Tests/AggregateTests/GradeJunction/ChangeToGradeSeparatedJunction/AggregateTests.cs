namespace RoadRegistry.Tests.AggregateTests.GradeJunction.ChangeToGradeSeparatedJunction;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;
using GradeJunction = RoadRegistry.GradeJunction.GradeJunction;

// 'Wijzig gelijkgrondse kruising naar ongelijkgrondse kruising': the crossing stays where it is, only what is recorded
// about it changes - from two roads meeting at grade to one passing over the other.
public class AggregateTests : AggregateTestBase
{
    private const int GradeJunctionId1 = 20;
    private const int RoadSegmentId1 = 1;
    private const int RoadSegmentId2 = 2;
    private const int UnrelatedRoadSegmentId = 3;

    private static readonly JunctionGeometry Crossing = JunctionGeometry.Create(
        new Point(new Coordinate(100, 80)) { SRID = WellknownSrids.Lambert08 });

    private GradeJunction BuildGradeJunction(bool removed = false)
    {
        var gradeJunction = GradeJunction.Create(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(GradeJunctionId1),
            RoadSegmentId1 = new RoadSegmentId(RoadSegmentId1),
            RoadSegmentId2 = new RoadSegmentId(RoadSegmentId2),
            Geometry = Crossing,
            Provenance = new ProvenanceData(TestData.Provenance)
        });

        if (removed)
        {
            gradeJunction.Remove(TestData.Provenance);
        }

        return gradeJunction.WithoutChanges();
    }

    private ScopedRoadNetwork BuildNetwork(GradeJunction gradeJunction)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [], [], [], [gradeJunction]).WithoutChanges();
    }

    private RoadNetworkChangeResult Act(
        ScopedRoadNetwork roadNetwork,
        int lowerRoadSegmentId = RoadSegmentId1,
        int upperRoadSegmentId = RoadSegmentId2,
        int gradeJunctionId = GradeJunctionId1,
        GradeSeparatedJunctionTypeV2? type = null)
    {
        return roadNetwork.ChangeGradeJunctionToGradeSeparatedJunction(
            new GradeJunctionId(gradeJunctionId),
            new RoadSegmentId(lowerRoadSegmentId),
            new RoadSegmentId(upperRoadSegmentId),
            type ?? GradeSeparatedJunctionTypeV2.Brug,
            IdGenerator(),
            TestData.Provenance);
    }

    private static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 100);
    }

    [Fact]
    public void WhenTheCrossingIsChanged_ThenTheGradeJunctionHandsItOverToAGradeSeparatedJunction()
    {
        var gradeJunction = BuildGradeJunction();
        var roadNetwork = BuildNetwork(gradeJunction);

        var result = Act(roadNetwork, type: GradeSeparatedJunctionTypeV2.Tunnel);

        result.Problems.Should().HaveNoError();

        var added = roadNetwork.GradeSeparatedJunctions.Values.Should().ContainSingle().Which;
        var addedEvent = added.GetChanges().OfType<GradeSeparatedJunctionWasAddedBecauseOfGradeJunctionChange>().Should().ContainSingle().Which;
        addedEvent.GradeJunctionId.Should().Be(new GradeJunctionId(GradeJunctionId1));
        addedEvent.LowerRoadSegmentId.Should().Be(new RoadSegmentId(RoadSegmentId1));
        addedEvent.UpperRoadSegmentId.Should().Be(new RoadSegmentId(RoadSegmentId2));
        addedEvent.Type.Should().Be(GradeSeparatedJunctionTypeV2.Tunnel);

        // The crossing point does not move: the new junction is the same place, recorded differently.
        addedEvent.Geometry.Should().Be(Crossing);

        var changedEvent = gradeJunction.GetChanges().OfType<GradeJunctionWasChangedToGradeSeparatedJunction>().Should().ContainSingle().Which;
        changedEvent.GradeSeparatedJunctionId.Should().Be(added.GradeSeparatedJunctionId);
        gradeJunction.IsRemoved.Should().BeTrue();
        gradeJunction.GetChanges().OfType<GradeJunctionWasRemoved>().Should().BeEmpty();
    }

    [Fact]
    public void WhenTheCrossingIsChanged_ThenTheChangeIsSummarised()
    {
        var roadNetwork = BuildNetwork(BuildGradeJunction());

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();

        var addedId = roadNetwork.GradeSeparatedJunctions.Values.Single().GradeSeparatedJunctionId;
        var summary = roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().ContainSingle().Which.Summary;
        summary.GradeJunctions.Removed.Should().BeEquivalentTo([GradeJunctionId1]);
        summary.GradeSeparatedJunctions.Added.Should().BeEquivalentTo([addedId.ToInt32()]);
    }

    // VAL-2
    [Fact]
    public void WhenTheGradeJunctionDoesNotExist_ThenItIsReported()
    {
        var roadNetwork = BuildNetwork(BuildGradeJunction());

        var result = Act(roadNetwork, gradeJunctionId: 999);

        result.Problems.Should().Contain(x => x.Reason == "GradeJunctionDoesNotExist");
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // VAL-3
    [Fact]
    public void WhenTheGradeJunctionIsRemoved_ThenItIsReported()
    {
        var roadNetwork = BuildNetwork(BuildGradeJunction(removed: true));

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "GradeJunctionIsRemoved");
        roadNetwork.GradeSeparatedJunctions.Should().BeEmpty();
    }

    // A crossing is about two roads, and a road whose inwinning is not finished is not one the register can say
    // anything about yet - not even that it passes under or over another.
    [Fact]
    public void WhenARoadSegmentHasNotCompletedItsInwinning_ThenItIsReported()
    {
        var gradeJunction = BuildGradeJunction();
        var notMigratedRoadSegment = RoadSegment.CreateForMigration(
            new RoadSegmentId(RoadSegmentId1),
            TestData.Segment1Added.Geometry,
            RoadSegmentStatusV2.Gerealiseerd,
            new RoadNodeId(1),
            new RoadNodeId(2));

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [], [notMigratedRoadSegment], [], [gradeJunction]).WithoutChanges();

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentNotCompletedInwinning");
        roadNetwork.GradeSeparatedJunctions.Should().BeEmpty();
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // VAL-5, VAL-7: the caller says which road goes under and which goes over, so both have to be roads this crossing
    // is actually about.
    [Theory]
    [InlineData(UnrelatedRoadSegmentId, RoadSegmentId2)]
    [InlineData(RoadSegmentId1, UnrelatedRoadSegmentId)]
    public void WhenARoadSegmentIsNotPartOfTheGradeJunction_ThenItIsReported(int lowerRoadSegmentId, int upperRoadSegmentId)
    {
        var roadNetwork = BuildNetwork(BuildGradeJunction());

        var result = Act(roadNetwork, lowerRoadSegmentId, upperRoadSegmentId);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentDoesNotBelongToGradeJunction");
        roadNetwork.GradeSeparatedJunctions.Should().BeEmpty();
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // VAL-8
    [Fact]
    public void WhenTheUpperAndLowerRoadSegmentAreTheSame_ThenItIsReported()
    {
        var roadNetwork = BuildNetwork(BuildGradeJunction());

        var result = Act(roadNetwork, RoadSegmentId1, RoadSegmentId1);

        result.Problems.Should().Contain(x => x.Reason == "GradeSeparatedJunctionUpperEqualsLowerRoadSegment");
        roadNetwork.GradeSeparatedJunctions.Should().BeEmpty();
    }
}
