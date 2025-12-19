namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class GradeSeparatedJunctionNotFound : Error
{
    public GradeSeparatedJunctionNotFound()
        : base(ProblemCode.GradeSeparatedJunction.NotFound)
    {
    }

    public GradeSeparatedJunctionNotFound(GradeSeparatedJunctionId gradeSeparatedJunctionId)
        : base(ProblemCode.GradeSeparatedJunction.NotFound,
            new ProblemParameter("Identifier", gradeSeparatedJunctionId.ToInt32().ToString()))
    {
        WithContext(ProblemContext.For(gradeSeparatedJunctionId));
    }
}
