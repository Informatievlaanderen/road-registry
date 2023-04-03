namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadNodeNotConnectedToAnySegment : Error
{
    public RoadNodeNotConnectedToAnySegment(RoadNodeId node)
        : base(ProblemCode.RoadNode.NotConnectedToAnySegment,
            new ProblemParameter("RoadNodeId", node.ToInt32().ToString()))
    {
    }
}
