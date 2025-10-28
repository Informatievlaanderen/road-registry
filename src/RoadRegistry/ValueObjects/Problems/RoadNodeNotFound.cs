namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadNodeNotFound : Error
{
    public RoadNodeNotFound()
        : base(ProblemCode.RoadNode.NotFound)
    {
    }

    public RoadNodeNotFound(RoadNodeId nodeId)
        : base(ProblemCode.RoadNode.NotFound,
            new ProblemParameter("NodeId", nodeId.ToInt32().ToString()))
    {
    }
}
