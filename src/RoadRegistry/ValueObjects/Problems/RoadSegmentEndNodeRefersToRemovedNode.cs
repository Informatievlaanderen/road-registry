namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentEndNodeRefersToRemovedNode : Error
{
    public RoadSegmentEndNodeRefersToRemovedNode(RoadSegmentId segment, RoadNodeId node)
        : base(ProblemCode.RoadSegment.EndNode.RefersToRemovedNode,
            new ProblemParameter("SegmentId", segment.ToInt32().ToString()),
            new ProblemParameter("NodeId", node.ToInt32().ToString()))
    {
    }
}
