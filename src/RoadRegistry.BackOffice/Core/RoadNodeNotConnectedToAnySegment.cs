namespace RoadRegistry.BackOffice.Core;

public class RoadNodeNotConnectedToAnySegment : Error
{
    public RoadNodeNotConnectedToAnySegment(RoadNodeId node)
        : base(nameof(RoadNodeNotConnectedToAnySegment),
            new ProblemParameter("RoadNodeId", node.ToInt32().ToString()))
    {
    }
}