namespace RoadRegistry.Tests.AggregateTests.RoadNode.ModifyRoadNodeAttributes;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

// 'Wijzig attribuutwaarden' for road nodes. Today that is 'grensknoop' alone: the road node type is derived from the
// network and is never named by hand.
public class AggregateTests : AggregateTestBase
{
    private const int RoadNodeId1 = 10;
    private const int RoadNodeId2 = 11;

    private RoadNodeWasAdded BuildNode(int id, bool grensknoop)
    {
        return new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(id),
            Geometry = new Point(new Coordinate(100, id)) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry(),
            Grensknoop = grensknoop,
            Type = RoadNodeTypeV2.Eindknoop,
            Provenance = new ProvenanceData(TestData.Provenance)
        };
    }

    private ScopedRoadNetwork BuildNetwork(params RoadNode[] roadNodes)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), roadNodes, [], [], []).WithoutChanges();
    }

    private RoadNode Existing(RoadNodeWasAdded added)
    {
        return RoadNode.Create(added).WithoutChanges();
    }

    private RoadNode Removed(RoadNodeWasAdded added)
    {
        var roadNode = RoadNode.Create(added);
        roadNode.Remove(TestData.Provenance);
        return roadNode.WithoutChanges();
    }

    private static ModifyRoadNodeChange Grensknoop(int roadNodeId, bool grensknoop)
    {
        return new ModifyRoadNodeChange
        {
            RoadNodeId = new RoadNodeId(roadNodeId),
            Grensknoop = grensknoop
        };
    }

    [Fact]
    public void WhenTheValueDiffers_ThenTheRoadNodeIsModified()
    {
        var roadNetwork = BuildNetwork(Existing(BuildNode(RoadNodeId1, grensknoop: false)));

        var result = roadNetwork.ModifyRoadNodeAttributes([Grensknoop(RoadNodeId1, true)], TestData.Provenance);

        result.Problems.Should().HaveNoError();

        var roadNode = roadNetwork.RoadNodes[new RoadNodeId(RoadNodeId1)];
        roadNode.Grensknoop.Should().BeTrue();
        roadNode.GetChanges().OfType<RoadNodeWasModified>().Should().ContainSingle()
            .Which.Grensknoop.Should().BeTrue();

        var summary = roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().ContainSingle().Which.Summary;
        summary.RoadNodes.Modified.Should().BeEquivalentTo([RoadNodeId1]);
    }

    // Both directions, since that is what the story asks to confirm.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WhenTheValueIsTurnedAround_ThenItTakesTheNewValue(bool grensknoop)
    {
        var roadNetwork = BuildNetwork(Existing(BuildNode(RoadNodeId1, grensknoop: !grensknoop)));

        var result = roadNetwork.ModifyRoadNodeAttributes([Grensknoop(RoadNodeId1, grensknoop)], TestData.Provenance);

        result.Problems.Should().HaveNoError();
        roadNetwork.RoadNodes[new RoadNodeId(RoadNodeId1)].Grensknoop.Should().Be(grensknoop);
    }

    // One call, several road nodes: what the bulk endpoint is for.
    [Fact]
    public void WhenSeveralRoadNodesAreNamed_ThenTheyAreAllChanged()
    {
        var roadNetwork = BuildNetwork(
            Existing(BuildNode(RoadNodeId1, grensknoop: false)),
            Existing(BuildNode(RoadNodeId2, grensknoop: false)));

        var result = roadNetwork.ModifyRoadNodeAttributes(
            [Grensknoop(RoadNodeId1, true), Grensknoop(RoadNodeId2, true)],
            TestData.Provenance);

        result.Problems.Should().HaveNoError();
        roadNetwork.RoadNodes[new RoadNodeId(RoadNodeId1)].Grensknoop.Should().BeTrue();
        roadNetwork.RoadNodes[new RoadNodeId(RoadNodeId2)].Grensknoop.Should().BeTrue();

        var summary = roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().ContainSingle().Which.Summary;
        summary.RoadNodes.Modified.Should().BeEquivalentTo([RoadNodeId1, RoadNodeId2]);
    }

    // Asking for the value it already has changes nothing, so there is no event and nothing to summarise.
    [Fact]
    public void WhenTheValueIsAlreadyWhatIsAsked_ThenNothingHappens()
    {
        var roadNetwork = BuildNetwork(Existing(BuildNode(RoadNodeId1, grensknoop: true)));

        var result = roadNetwork.ModifyRoadNodeAttributes([Grensknoop(RoadNodeId1, true)], TestData.Provenance);

        result.Problems.Should().HaveNoError();
        roadNetwork.RoadNodes[new RoadNodeId(RoadNodeId1)].GetChanges().Should().BeEmpty();
        roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().BeEmpty();
    }

    // VAL-4
    [Fact]
    public void WhenTheRoadNodeIsNotFound_ThenItIsReported()
    {
        var roadNetwork = BuildNetwork(Existing(BuildNode(RoadNodeId1, grensknoop: false)));

        var result = roadNetwork.ModifyRoadNodeAttributes([Grensknoop(RoadNodeId2, true)], TestData.Provenance);

        result.Problems.Should().Contain(x => x.Reason == "RoadNodeChangeAttributesNotFound");
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // VAL-5
    [Fact]
    public void WhenTheRoadNodeIsRemoved_ThenItIsReported()
    {
        var roadNetwork = BuildNetwork(Removed(BuildNode(RoadNodeId1, grensknoop: false)));

        var result = roadNetwork.ModifyRoadNodeAttributes([Grensknoop(RoadNodeId1, true)], TestData.Provenance);

        result.Problems.Should().Contain(x => x.Reason == "RoadNodeChangeAttributesIsRemoved");
        roadNetwork.RoadNodes[new RoadNodeId(RoadNodeId1)].Grensknoop.Should().BeFalse();
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // The whole request is refused rather than half-applied: one bad road node holds back the good one.
    [Fact]
    public void WhenOneOfTheRoadNodesIsWrong_ThenNothingIsChanged()
    {
        var roadNetwork = BuildNetwork(Existing(BuildNode(RoadNodeId1, grensknoop: false)));

        var result = roadNetwork.ModifyRoadNodeAttributes(
            [Grensknoop(RoadNodeId1, true), Grensknoop(RoadNodeId2, true)],
            TestData.Provenance);

        result.Problems.Should().Contain(x => x.Reason == "RoadNodeChangeAttributesNotFound");
        roadNetwork.RoadNodes[new RoadNodeId(RoadNodeId1)].Grensknoop.Should().BeFalse();
        roadNetwork.GetChanges().Should().BeEmpty();
    }
}
