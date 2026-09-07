namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RemoveRoadSegments;

using System.Linq;
using FluentAssertions;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;

// Whether a node that is no longer needed takes the two segments hanging off it with it is not a default any more -
// every caller of VerifyTopologyAndUpdateType says which it wants. These two tests are the same road network under
// the two answers, through the actions that give them: removing a segment merges, historeren leaves the node alone.
public class NodeTopologyMergeTests : RemoveRoadSegmentsTestBase
{
    private const int LeavingSegmentId = 1;

    // A three-way node: once the eastern arm leaves, the two that are left run straight through and differ in nothing
    // that is not a dynamic attribute, so nothing about them stands in the way of merging.
    private ScopedRoadNetwork.ScopedRoadNetwork BuildThreeWayNode()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 80, RoadNodeTypeV2.EchteKnoop);
        var northNode = BuildNode(12, 100, 160, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 80, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [southNode, middleNode, northNode, eastNode],
            [
                BuildSegment(LeavingSegmentId, middleNode, eastNode, BuildGeometry((100, 80), (200, 80))),
                BuildSegment(2, southNode, middleNode, BuildGeometry((100, 0), (100, 80))),
                BuildSegment(3, middleNode, northNode, BuildGeometry((100, 80), (100, 160)))
            ]);
    }

    [Fact]
    public void WhenTheSegmentIsRemoved_ThenTheTwoLeftAtTheNodeAreMerged()
    {
        var roadNetwork = BuildThreeWayNode();

        roadNetwork.RemoveRoadSegments([new RoadSegmentId(LeavingSegmentId)], IdGenerator(), TestData.Provenance);

        roadNetwork.RoadSegments.Values.SelectMany(x => x.GetChanges()).OfType<RoadSegmentWasMerged>().Should().ContainSingle();
        roadNetwork.RoadNodes[new RoadNodeId(11)].IsRemoved.Should().BeTrue();
    }

    // The same shape historeerd instead: that action names one segment and has to leave the roads around it as they
    // are, so the node keeps both of them and becomes the validatieknoop between them.
    [Fact]
    public void WhenTheSegmentIsHistorized_ThenTheTwoLeftAtTheNodeAreNotMerged()
    {
        var roadNetwork = BuildThreeWayNode();

        var result = roadNetwork.ChangeRoadSegmentStatus(
            RoadSegmentStatusChange.RealizedToHistorized,
            new RoadSegmentId(LeavingSegmentId),
            mayModifyMeasuredRoadSegments: true,
            IdGenerator(),
            TestData.Provenance);

        result.Problems.Should().HaveNoError();
        roadNetwork.RoadSegments.Values.SelectMany(x => x.GetChanges()).OfType<RoadSegmentWasMerged>().Should().BeEmpty();
        roadNetwork.RoadNodes[new RoadNodeId(11)].IsRemoved.Should().BeFalse();
        roadNetwork.RoadNodes[new RoadNodeId(11)].Type.Should().Be(RoadNodeTypeV2.Validatieknoop);
    }
}
