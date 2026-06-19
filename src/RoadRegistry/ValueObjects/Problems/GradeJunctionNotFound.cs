namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class GradeJunctionNotFound : Error
{
    public GradeJunctionNotFound()
        : base(ProblemCode.GradeJunction.NotFound.ToString())
    {
    }

    public GradeJunctionNotFound(GradeJunctionId gradeJunctionId)
        : base(ProblemCode.GradeJunction.NotFound.ToString(),
            new ProblemParameter("Identifier", gradeJunctionId.ToInt32().ToString()))
    {
        WithContext(ProblemContext.For(gradeJunctionId));
    }
}
