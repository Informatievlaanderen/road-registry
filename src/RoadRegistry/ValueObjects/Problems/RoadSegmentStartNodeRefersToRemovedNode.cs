namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentStartNodeRefersToRemovedNode : Error
{
    public RoadSegmentStartNodeRefersToRemovedNode(RoadSegmentId segment, RoadNodeId node)
        : base(ProblemCode.RoadSegment.StartNode.RefersToRemovedNode,
            new ProblemParameter("SegmentId", segment.ToInt32().ToString()),
            new ProblemParameter("NodeId", node.ToInt32().ToString()))
    {
    }
}
