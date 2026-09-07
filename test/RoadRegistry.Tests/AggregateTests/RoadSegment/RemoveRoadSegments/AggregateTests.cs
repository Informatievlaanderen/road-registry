namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RemoveRoadSegments;

using System.Linq;
using FluentAssertions;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;

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

    // VAL-2, and reported for every identifier the request names rather than the first one that is wrong.
    [Fact]
    public void WhenTheSegmentsAreNotFound_ThenEveryOneOfThemIsReported()
    {
        var roadNetwork = BuildNetwork([], []);

        var act = () => roadNetwork.RemoveRoadSegments(
            [new RoadSegmentId(RemovedSegmentId), new RoadSegmentId(NeighbourSegmentId)],
            IdGenerator(),
            TestData.Provenance);

        var problems = act.Should().Throw<RoadRegistryProblemsException>().Which.Problems;
        problems.Should().HaveCount(2);
        problems.Select(x => x.Reason).Should().AllBe(ProblemCode.RoadSegment.NotFound.ToString());
    }
}
