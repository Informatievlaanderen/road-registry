namespace RoadRegistry.ValueObjects.Problems;

using RoadRegistry.ValueObjects.ProblemCodes;

public class GradeSeparatedJunctionNotUnique : Error
{
    public GradeSeparatedJunctionNotUnique(GradeSeparatedJunctionId gradeSeparatedJunctionId, GradeSeparatedJunctionId otherGradeSeparatedJunctionId)
        : base(ProblemCode.GradeSeparatedJunction.NotUnique,
            new ProblemParameter("GradeSeparatedJunctionId", gradeSeparatedJunctionId.ToString()),
            new ProblemParameter("OtherGradeSeparatedJunctionId", otherGradeSeparatedJunctionId.ToString())
        )
    {
    }
}
