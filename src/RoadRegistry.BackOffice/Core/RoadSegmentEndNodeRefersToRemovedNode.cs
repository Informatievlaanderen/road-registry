namespace RoadRegistry.BackOffice.Core;

public class RoadSegmentEndNodeRefersToRemovedNode : Error
{
    public RoadSegmentEndNodeRefersToRemovedNode(RoadSegmentId segment, RoadNodeId node)
        : base(nameof(RoadSegmentEndNodeRefersToRemovedNode),
            new ProblemParameter("SegmentId", segment.ToInt32().ToString()),
            new ProblemParameter("NodeId", node.ToInt32().ToString()))
    {
    }
}
