namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-3: the grade separated junction is still known but has been removed, so there is nothing left to change about it.
public class GradeSeparatedJunctionIsRemoved : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.GradeSeparatedJunction.IsRemoved;

    public GradeSeparatedJunctionIsRemoved(GradeSeparatedJunctionId identifier)
        : base(ProblemCode,
            new ProblemParameter("OngelijkGrondseKruisingId", identifier.ToInt32().ToString()))
    {
    }
}
