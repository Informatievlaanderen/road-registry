namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadNodeGeometryTaken : Error
{
    public RoadNodeGeometryTaken(RoadNodeId byOtherNode)
        : base(ProblemCode.RoadNode.Geometry.Taken,
        new ProblemParameter("ByOtherNode", byOtherNode.ToInt32().ToString()))
    {
    }
}
