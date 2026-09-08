namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-3: the grade junction is still known but has been removed, so there is nothing left to change about it.
public class GradeJunctionIsRemoved : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.GradeJunction.IsRemoved;

    public GradeJunctionIsRemoved(GradeJunctionId identifier)
        : base(ProblemCode,
            new ProblemParameter("GelijkGrondseKruisingId", identifier.ToInt32().ToString()))
    {
    }
}
