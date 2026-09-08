namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.ChangeToGradeJunction;

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
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;

// 'Wijzig ongelijkgrondse kruising naar gelijkgrondse kruising': the bridge or tunnel is gone from the terrain, or was
// recorded in error, and the crossing is at grade after all.
public class AggregateTests : AggregateTestBase
{
    private const int GradeSeparatedJunctionId1 = 30;
    private const int LowerRoadSegmentId = 1;
    private const int UpperRoadSegmentId = 2;

    private static readonly JunctionGeometry Crossing = JunctionGeometry.Create(
        new Point(new Coordinate(100, 80)) { SRID = WellknownSrids.Lambert08 });

    private GradeSeparatedJunction BuildGradeSeparatedJunction(bool removed = false)
    {
        var junction = GradeSeparatedJunction.Create(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(GradeSeparatedJunctionId1),
            LowerRoadSegmentId = new RoadSegmentId(LowerRoadSegmentId),
            UpperRoadSegmentId = new RoadSegmentId(UpperRoadSegmentId),
            Type = GradeSeparatedJunctionTypeV2.Brug,
            Geometry = Crossing,
            Provenance = new ProvenanceData(TestData.Provenance)
        });

        if (removed)
        {
            junction.Remove(TestData.Provenance);
        }

        return junction.WithoutChanges();
    }

    private ScopedRoadNetwork BuildNetwork(GradeSeparatedJunction gradeSeparatedJunction)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [], [], [gradeSeparatedJunction], []).WithoutChanges();
    }

    private RoadNetworkChangeResult Act(ScopedRoadNetwork roadNetwork, int gradeSeparatedJunctionId = GradeSeparatedJunctionId1)
    {
        return roadNetwork.ChangeGradeSeparatedJunctionToGradeJunction(
            new GradeSeparatedJunctionId(gradeSeparatedJunctionId),
            new InMemoryRoadNetworkIdGenerator(initialValue: 100),
            TestData.Provenance);
    }

    [Fact]
    public void WhenTheCrossingIsChanged_ThenTheGradeSeparatedJunctionHandsItOverToAGradeJunction()
    {
        var gradeSeparatedJunction = BuildGradeSeparatedJunction();
        var roadNetwork = BuildNetwork(gradeSeparatedJunction);

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();

        var added = roadNetwork.GradeJunctions.Values.Should().ContainSingle().Which;
        var addedEvent = added.GetChanges().OfType<GradeJunctionWasAddedBecauseOfGradeSeparatedJunctionChange>().Should().ContainSingle().Which;
        addedEvent.GradeSeparatedJunctionId.Should().Be(new GradeSeparatedJunctionId(GradeSeparatedJunctionId1));

        // Which road becomes 'wegsegment 1' and which 'wegsegment 2' does not matter, but both roads of the crossing
        // have to come along.
        new[] { addedEvent.RoadSegmentId1, addedEvent.RoadSegmentId2 }.Should()
            .BeEquivalentTo([new RoadSegmentId(LowerRoadSegmentId), new RoadSegmentId(UpperRoadSegmentId)]);

        // The crossing point does not move: the new junction is the same place, recorded differently.
        addedEvent.Geometry.Should().Be(Crossing);

        var changedEvent = gradeSeparatedJunction.GetChanges().OfType<GradeSeparatedJunctionWasChangedToGradeJunction>().Should().ContainSingle().Which;
        changedEvent.GradeJunctionId.Should().Be(added.GradeJunctionId);
        gradeSeparatedJunction.IsRemoved.Should().BeTrue();
        gradeSeparatedJunction.GetChanges().OfType<GradeSeparatedJunctionWasRemoved>().Should().BeEmpty();
    }

    [Fact]
    public void WhenTheCrossingIsChanged_ThenTheChangeIsSummarised()
    {
        var roadNetwork = BuildNetwork(BuildGradeSeparatedJunction());

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();

        var addedId = roadNetwork.GradeJunctions.Values.Single().GradeJunctionId;
        var summary = roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().ContainSingle().Which.Summary;
        summary.GradeSeparatedJunctions.Removed.Should().BeEquivalentTo([GradeSeparatedJunctionId1]);
        summary.GradeJunctions.Added.Should().BeEquivalentTo([addedId.ToInt32()]);
    }

    // A crossing is about two roads, and a road whose inwinning is not finished is not one the register can say
    // anything about yet.
    [Fact]
    public void WhenARoadSegmentHasNotCompletedItsInwinning_ThenItIsReported()
    {
        var gradeSeparatedJunction = BuildGradeSeparatedJunction();
        var notMigratedRoadSegment = RoadSegment.CreateForMigration(
            new RoadSegmentId(LowerRoadSegmentId),
            TestData.Segment1Added.Geometry,
            RoadSegmentStatusV2.Gerealiseerd,
            new RoadNodeId(1),
            new RoadNodeId(2));

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [], [notMigratedRoadSegment], [gradeSeparatedJunction], []).WithoutChanges();

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentNotCompletedInwinning");
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // VAL-2
    [Fact]
    public void WhenTheGradeSeparatedJunctionDoesNotExist_ThenItIsReported()
    {
        var roadNetwork = BuildNetwork(BuildGradeSeparatedJunction());

        var result = Act(roadNetwork, gradeSeparatedJunctionId: 999);

        result.Problems.Should().Contain(x => x.Reason == "GradeSeparatedJunctionDoesNotExist");
        roadNetwork.GetChanges().Should().BeEmpty();
        roadNetwork.GradeJunctions.Should().BeEmpty();
    }

    // VAL-3
    [Fact]
    public void WhenTheGradeSeparatedJunctionIsRemoved_ThenItIsReported()
    {
        var roadNetwork = BuildNetwork(BuildGradeSeparatedJunction(removed: true));

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "GradeSeparatedJunctionIsRemoved");
        roadNetwork.GradeJunctions.Should().BeEmpty();
    }
}
