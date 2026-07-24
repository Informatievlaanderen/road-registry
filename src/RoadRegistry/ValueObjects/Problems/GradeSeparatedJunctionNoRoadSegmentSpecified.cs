namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// A grade-separated junction modification must repoint at least one of its two road segments; a modification with neither side set has nothing to change.
public class GradeSeparatedJunctionNoRoadSegmentSpecified : Error
{
    public GradeSeparatedJunctionNoRoadSegmentSpecified()
        : base(ProblemCode.GradeSeparatedJunction.NoRoadSegmentSpecified.ToString())
    {
    }

    public GradeSeparatedJunctionNoRoadSegmentSpecified(GradeSeparatedJunctionId gradeSeparatedJunctionId)
        : base(ProblemCode.GradeSeparatedJunction.NoRoadSegmentSpecified,
            new ProblemParameter("Identifier", gradeSeparatedJunctionId.ToInt32().ToString()))
    {
        WithContext(ProblemContext.For(gradeSeparatedJunctionId));
    }
}
