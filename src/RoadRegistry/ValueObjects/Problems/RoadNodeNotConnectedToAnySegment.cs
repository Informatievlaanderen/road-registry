namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadNodeNotConnectedToAnySegment : Error
{
    public RoadNodeNotConnectedToAnySegment()
        : base(ProblemCode.RoadNode.NotConnectedToAnySegment.ToString())
    {
    }

    public RoadNodeNotConnectedToAnySegment(RoadNodeId node)
        : base(ProblemCode.RoadNode.NotConnectedToAnySegment,
            new ProblemParameter("RoadNodeId", node.ToInt32().ToString()))
    {
    }
}
