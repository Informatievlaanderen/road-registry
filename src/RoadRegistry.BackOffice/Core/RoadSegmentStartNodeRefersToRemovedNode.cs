namespace RoadRegistry.BackOffice.Core;

public class RoadSegmentStartNodeRefersToRemovedNode : Error
{
    public RoadSegmentStartNodeRefersToRemovedNode(RoadSegmentId segment, RoadNodeId node)
        : base(nameof(RoadSegmentStartNodeRefersToRemovedNode),
            new ProblemParameter("SegmentId", segment.ToInt32().ToString()),
            new ProblemParameter("NodeId", node.ToInt32().ToString()))
    {
    }
}