namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadNodeNotFound : Error
{
    public RoadNodeNotFound()
        : base(ProblemCode.RoadNode.NotFound)
    {
    }
}
