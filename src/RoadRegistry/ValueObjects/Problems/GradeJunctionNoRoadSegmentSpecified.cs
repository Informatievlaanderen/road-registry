namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// A grade junction modification must repoint at least one of its two road segments; a modification with neither side set has nothing to change.
public class GradeJunctionNoRoadSegmentSpecified : Error
{
    public GradeJunctionNoRoadSegmentSpecified()
        : base(ProblemCode.GradeJunction.NoRoadSegmentSpecified.ToString())
    {
    }

    public GradeJunctionNoRoadSegmentSpecified(GradeJunctionId gradeJunctionId)
        : base(ProblemCode.GradeJunction.NoRoadSegmentSpecified,
            new ProblemParameter("Identifier", gradeJunctionId.ToInt32().ToString()))
    {
        WithContext(ProblemContext.For(gradeJunctionId));
    }
}
