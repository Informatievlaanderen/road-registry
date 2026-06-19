namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class GradeJunctionNotFound : Error
{
    public GradeJunctionNotFound()
        : base(ProblemCode.GradeJunction.NotFound.ToString())
    {
    }

    public GradeJunctionNotFound(GradeSeparatedJunctionId gradeSeparatedJunctionId)
        : base(ProblemCode.GradeSeparatedJunction.NotFound.ToString(),
            new ProblemParameter("Identifier", gradeSeparatedJunctionId.ToInt32().ToString()))
    {
        WithContext(ProblemContext.For(gradeSeparatedJunctionId));
    }
}
