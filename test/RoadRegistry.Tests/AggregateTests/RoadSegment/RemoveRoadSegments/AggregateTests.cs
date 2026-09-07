namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RemoveRoadSegments;

using System;
using System.Linq;
using FluentAssertions;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.Events.V2;

public class AggregateTests : RemoveRoadSegmentsTestBase
{
    private const int RemovedSegmentId = 1;
    private const int NeighbourSegmentId = 2;

    // A segment on its own between two end nodes: nothing else hangs off either node, so both go with it.
    [Fact]
    public void WhenASegmentOnItsOwnIsRemoved_ThenItsRoadNodesGoWithIt()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var roadNetwork = BuildNetwork(
            [southNode, northNode],
            [BuildSegment(RemovedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)))]);

        roadNetwork.RemoveRoadSegments([new RoadSegmentId(RemovedSegmentId)], IdGenerator(), TestData.Provenance);

        roadNetwork.RoadSegments[new RoadSegmentId(RemovedSegmentId)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadNodes[new RoadNodeId(10)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadNodes[new RoadNodeId(11)].IsRemoved.Should().BeTrue();
    }

    // The category no longer decides: a central manager removes whatever they name, a European main road included.
    [Theory]
    [InlineData(nameof(RoadSegmentCategoryV2.EuropeseHoofdweg))]
    [InlineData(nameof(RoadSegmentCategoryV2.VlaamseHoofdweg))]
    public void WhenTheSegmentCarriesACategoryThatUsedToBeRefused_ThenItIsStillRemoved(string category)
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var roadNetwork = BuildNetwork(
            [southNode, northNode],
            [BuildSegment(RemovedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)), category: RoadSegmentCategoryV2.Parse(category))]);

        roadNetwork.RemoveRoadSegments([new RoadSegmentId(RemovedSegmentId)], IdGenerator(), TestData.Provenance);

        roadNetwork.RoadSegments[new RoadSegmentId(RemovedSegmentId)].IsRemoved.Should().BeTrue();
    }

    // Two segments are left hanging off the node and they differ in nothing, so the node is not needed any more:
    // the two become one and the validatieknoop between them disappears. This is the 'samenvoegen' step.
    [Fact]
    public void WhenANodeIsLeftWithTwoSegmentsThatCanBeMerged_ThenTheyAreMerged()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 80, RoadNodeTypeV2.EchteKnoop);
        var northNode = BuildNode(12, 100, 160, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 80, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [southNode, middleNode, northNode, eastNode],
            [
                BuildSegment(RemovedSegmentId, middleNode, eastNode, BuildGeometry((100, 80), (200, 80))),
                BuildSegment(NeighbourSegmentId, southNode, middleNode, BuildGeometry((100, 0), (100, 80))),
                BuildSegment(3, middleNode, northNode, BuildGeometry((100, 80), (100, 160)))
            ]);

        // The three-way node loses one of its three segments; the two that are left differ in nothing, so they merge
        // and the node they met at is no longer needed.
        roadNetwork.RemoveRoadSegments([new RoadSegmentId(RemovedSegmentId)], IdGenerator(), TestData.Provenance);

        roadNetwork.RoadNodes[new RoadNodeId(13)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadSegments[new RoadSegmentId(RemovedSegmentId)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadSegments.Values.SelectMany(x => x.GetChanges()).OfType<RoadSegmentWasMerged>().Should().ContainSingle();
    }

    // One segment left hanging off the node: a road that stops there ends there, so the node becomes an end node.
    [Fact]
    public void WhenANodeIsLeftWithOneSegment_ThenItBecomesAnEindknoop()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Validatieknoop);
        var northNode = BuildNode(12, 100, 160, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 80, RoadNodeTypeV2.Eindknoop);

        // Three segments meet at the middle node, so removing one leaves two - and those two cannot merge, because
        // the third arm makes the node a real one. Removing the northern arm as well would.
        var roadNetwork = BuildNetwork(
            [southNode, middleNode, northNode, eastNode],
            [
                BuildSegment(RemovedSegmentId, middleNode, northNode, BuildGeometry((100, 80), (100, 160))),
                BuildSegment(NeighbourSegmentId, southNode, middleNode, BuildGeometry((100, 0), (100, 80))),
                BuildSegment(3, middleNode, eastNode, BuildGeometry((100, 80), (200, 80)))
            ]);

        roadNetwork.RemoveRoadSegments([new RoadSegmentId(RemovedSegmentId), new RoadSegmentId(3)], IdGenerator(), TestData.Provenance);

        roadNetwork.RoadNodes[new RoadNodeId(11)].IsRemoved.Should().BeFalse();
        roadNetwork.RoadNodes[new RoadNodeId(11)].Type.Should().Be(RoadNodeTypeV2.Eindknoop);
        roadNetwork.RoadNodes[new RoadNodeId(12)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadNodes[new RoadNodeId(13)].IsRemoved.Should().BeTrue();
    }

    // Both segments meeting at the node are named in the same request, so nothing is merged on the way: the node and
    // both segments simply go. Merging per segment would have joined them and then removed half of that merge again.
    [Fact]
    public void WhenTwoSegmentsMeetingAtANodeAreBothRemoved_ThenNeitherIsMergedFirst()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Validatieknoop);
        var northNode = BuildNode(12, 100, 160, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [southNode, middleNode, northNode],
            [
                BuildSegment(RemovedSegmentId, southNode, middleNode, BuildGeometry((100, 0), (100, 80))),
                BuildSegment(NeighbourSegmentId, middleNode, northNode, BuildGeometry((100, 80), (100, 160)))
            ]);

        roadNetwork.RemoveRoadSegments(
            [new RoadSegmentId(RemovedSegmentId), new RoadSegmentId(NeighbourSegmentId)],
            IdGenerator(),
            TestData.Provenance);

        roadNetwork.RoadSegments[new RoadSegmentId(RemovedSegmentId)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadSegments[new RoadSegmentId(NeighbourSegmentId)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadNodes[new RoadNodeId(11)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadSegments.Values.SelectMany(x => x.GetChanges()).OfType<RoadSegmentWasMerged>().Should().BeEmpty();
    }

    // Removing what is not there is what the caller asked for, so it is not an error - and with nothing changed there
    // is nothing to report either.
    [Fact]
    public void WhenTheSegmentsAreNotFound_ThenNothingHappens()
    {
        var roadNetwork = BuildNetwork([], []);

        var act = () => roadNetwork.RemoveRoadSegments(
            [new RoadSegmentId(RemovedSegmentId), new RoadSegmentId(NeighbourSegmentId)],
            IdGenerator(),
            TestData.Provenance);

        act.Should().NotThrow();
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // The same segment reached twice within one request: the second time it is already gone, which is not an error and
    // not a second removal either.
    [Fact]
    public void WhenTheSegmentIsNamedTwice_ThenItIsRemovedOnce()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var roadNetwork = BuildNetwork(
            [southNode, northNode],
            [BuildSegment(RemovedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)))]);

        var act = () => roadNetwork.RemoveRoadSegments(
            [new RoadSegmentId(RemovedSegmentId), new RoadSegmentId(RemovedSegmentId)],
            IdGenerator(),
            TestData.Provenance);

        act.Should().NotThrow();

        var segment = roadNetwork.RoadSegments[new RoadSegmentId(RemovedSegmentId)];
        segment.IsRemoved.Should().BeTrue();
        segment.GetChanges().OfType<RoadSegmentWasRemoved>().Should().ContainSingle();

        var summary = roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().ContainSingle().Which.Summary;
        summary.RoadSegments.Removed.Should().BeEquivalentTo([RemovedSegmentId]);
    }

    // The summary the ticket reports back, and the event that carries it.
    [Fact]
    public void WhenSegmentsAreRemoved_ThenTheChangeIsSummarised()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var roadNetwork = BuildNetwork(
            [southNode, northNode],
            [BuildSegment(RemovedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)))]);

        roadNetwork.RemoveRoadSegments([new RoadSegmentId(RemovedSegmentId)], IdGenerator(), TestData.Provenance);

        var summary = roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().ContainSingle().Which.Summary;
        summary.RoadSegments.Removed.Should().BeEquivalentTo([RemovedSegmentId]);
        summary.RoadNodes.Removed.Should().BeEquivalentTo([10, 11]);
    }

    // A node that is left standing but no longer means the same thing changed too, and the ticket has to say so.
    [Fact]
    public void WhenANodeIsRetyped_ThenItIsSummarisedAsModified()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 80, RoadNodeTypeV2.EchteKnoop);
        var northNode = BuildNode(12, 100, 160, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 80, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [southNode, middleNode, northNode, eastNode],
            [
                BuildSegment(RemovedSegmentId, middleNode, northNode, BuildGeometry((100, 80), (100, 160))),
                BuildSegment(NeighbourSegmentId, southNode, middleNode, BuildGeometry((100, 0), (100, 80))),
                BuildSegment(3, middleNode, eastNode, BuildGeometry((100, 80), (200, 80)))
            ]);

        // Two of the three arms go, so the middle node is left carrying one road that now ends there: it stays put as
        // an eindknoop instead of the echte knoop it was, and the two nodes at the far ends go.
        roadNetwork.RemoveRoadSegments([new RoadSegmentId(RemovedSegmentId), new RoadSegmentId(3)], IdGenerator(), TestData.Provenance);

        roadNetwork.RoadNodes[new RoadNodeId(11)].Type.Should().Be(RoadNodeTypeV2.Eindknoop);

        var summary = roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().ContainSingle().Which.Summary;
        summary.RoadNodes.Modified.Should().BeEquivalentTo([11]);
        summary.RoadNodes.Removed.Should().BeEquivalentTo([12, 13]);
    }

    // A realized segment always hangs off two nodes and they are loaded with it, so a node it names that the network
    // does not have is a corrupt aggregate, not something the caller did - and it says so rather than quietly leaving
    // that node behind at a type that no longer fits.
    [Fact]
    public void WhenARoadNodeTheSegmentNamesIsMissing_ThenItThrows()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var roadNetwork = BuildNetwork(
            [],
            [BuildSegment(RemovedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)))]);

        var act = () => roadNetwork.RemoveRoadSegments([new RoadSegmentId(RemovedSegmentId)], IdGenerator(), TestData.Provenance);

        act.Should().Throw<InvalidOperationException>().WithMessage("*is not part of the road network*");
    }

    // Nothing changed, so there is nothing to summarise: an event saying so would have the projections apply a summary
    // with nothing in it and the ticket report a change that never happened.
    [Fact]
    public void WhenNothingIsRemoved_ThenNoSummaryEventIsRaised()
    {
        var roadNetwork = BuildNetwork([], []);

        roadNetwork.RemoveRoadSegments([new RoadSegmentId(RemovedSegmentId)], IdGenerator(), TestData.Provenance);

        roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().BeEmpty();
    }
}
