namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.ModifyGradeSeparatedJunctionAttributes;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
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

// 'Wijzig attribuutwaarde(n) voor een ongelijkgrondse kruising': the two roads the wrong way round, or the wrong kind
// of crossing.
public class AggregateTests : AggregateTestBase
{
    private const int GradeSeparatedJunctionId1 = 30;
    private const int LowerRoadSegmentId = 1;
    private const int UpperRoadSegmentId = 2;
    private const int UnrelatedRoadSegmentId = 3;

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

    private RoadNetworkChangeResult Act(
        ScopedRoadNetwork roadNetwork,
        int lowerRoadSegmentId = UpperRoadSegmentId,
        int upperRoadSegmentId = LowerRoadSegmentId,
        int gradeSeparatedJunctionId = GradeSeparatedJunctionId1,
        GradeSeparatedJunctionTypeV2? type = null)
    {
        return roadNetwork.ModifyGradeSeparatedJunctionAttributes(
            new GradeSeparatedJunctionId(gradeSeparatedJunctionId),
            new RoadSegmentId(lowerRoadSegmentId),
            new RoadSegmentId(upperRoadSegmentId),
            type ?? GradeSeparatedJunctionTypeV2.Tunnel,
            TestData.Provenance);
    }

    // What the endpoint is for: the roads swap places and the kind changes, in one action.
    [Fact]
    public void WhenTheRoadSegmentsAreSwappedAndTheTypeChanges_ThenTheJunctionIsModified()
    {
        var gradeSeparatedJunction = BuildGradeSeparatedJunction();
        var roadNetwork = BuildNetwork(gradeSeparatedJunction);

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();

        var modified = gradeSeparatedJunction.GetChanges().OfType<GradeSeparatedJunctionWasModified>().Should().ContainSingle().Which;
        modified.LowerRoadSegmentId.Should().Be(new RoadSegmentId(UpperRoadSegmentId));
        modified.UpperRoadSegmentId.Should().Be(new RoadSegmentId(LowerRoadSegmentId));
        modified.Type.Should().Be(GradeSeparatedJunctionTypeV2.Tunnel);

        gradeSeparatedJunction.LowerRoadSegmentId.Should().Be(new RoadSegmentId(UpperRoadSegmentId));
        gradeSeparatedJunction.UpperRoadSegmentId.Should().Be(new RoadSegmentId(LowerRoadSegmentId));
        gradeSeparatedJunction.Type.Should().Be(GradeSeparatedJunctionTypeV2.Tunnel);

        var summary = roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().ContainSingle().Which.Summary;
        summary.GradeSeparatedJunctions.Modified.Should().BeEquivalentTo([GradeSeparatedJunctionId1]);
    }

    // Only the kind is wrong, and the roads stay as they are.
    [Fact]
    public void WhenOnlyTheTypeChanges_ThenTheJunctionIsModified()
    {
        var gradeSeparatedJunction = BuildGradeSeparatedJunction();
        var roadNetwork = BuildNetwork(gradeSeparatedJunction);

        var result = Act(roadNetwork, LowerRoadSegmentId, UpperRoadSegmentId, type: GradeSeparatedJunctionTypeV2.Tunnel);

        result.Problems.Should().HaveNoError();
        gradeSeparatedJunction.Type.Should().Be(GradeSeparatedJunctionTypeV2.Tunnel);
        gradeSeparatedJunction.LowerRoadSegmentId.Should().Be(new RoadSegmentId(LowerRoadSegmentId));
    }

    // Asking for what is already recorded is no correction, so there is no event and no new version date.
    [Fact]
    public void WhenNothingChanges_ThenNothingHappens()
    {
        var gradeSeparatedJunction = BuildGradeSeparatedJunction();
        var roadNetwork = BuildNetwork(gradeSeparatedJunction);

        var result = Act(roadNetwork, LowerRoadSegmentId, UpperRoadSegmentId, type: GradeSeparatedJunctionTypeV2.Brug);

        result.Problems.Should().HaveNoError();
        gradeSeparatedJunction.GetChanges().Should().BeEmpty();
        roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().BeEmpty();
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
    }

    // VAL-3
    [Fact]
    public void WhenTheGradeSeparatedJunctionIsRemoved_ThenItIsReported()
    {
        var roadNetwork = BuildNetwork(BuildGradeSeparatedJunction(removed: true));

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "GradeSeparatedJunctionIsRemoved");
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // VAL-5, VAL-7: naming a third road is not a correction, it is a different crossing.
    [Theory]
    [InlineData(UnrelatedRoadSegmentId, UpperRoadSegmentId)]
    [InlineData(LowerRoadSegmentId, UnrelatedRoadSegmentId)]
    public void WhenARoadSegmentIsNotPartOfTheJunction_ThenItIsReported(int lowerRoadSegmentId, int upperRoadSegmentId)
    {
        var gradeSeparatedJunction = BuildGradeSeparatedJunction();
        var roadNetwork = BuildNetwork(gradeSeparatedJunction);

        var result = Act(roadNetwork, lowerRoadSegmentId, upperRoadSegmentId);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentDoesNotBelongToGradeSeparatedJunction");
        gradeSeparatedJunction.GetChanges().Should().BeEmpty();
    }

    // VAL-8
    [Fact]
    public void WhenTheUpperAndLowerRoadSegmentAreTheSame_ThenItIsReported()
    {
        var gradeSeparatedJunction = BuildGradeSeparatedJunction();
        var roadNetwork = BuildNetwork(gradeSeparatedJunction);

        var result = Act(roadNetwork, LowerRoadSegmentId, LowerRoadSegmentId);

        result.Problems.Should().Contain(x => x.Reason == "GradeSeparatedJunctionUpperEqualsLowerRoadSegment");
        gradeSeparatedJunction.GetChanges().Should().BeEmpty();
    }
}
