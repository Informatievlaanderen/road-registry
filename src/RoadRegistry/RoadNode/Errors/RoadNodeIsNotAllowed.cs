namespace RoadRegistry.RoadNode.Errors;

using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

public class RoadNodeIsNotAllowed : Error
{
    public RoadNodeIsNotAllowed()
        : base(ProblemCode.RoadNode.IsNotAllowed.ToString())
    {
    }
}
