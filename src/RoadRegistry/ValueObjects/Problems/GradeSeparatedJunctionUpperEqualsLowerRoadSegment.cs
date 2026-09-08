namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-8: a crossing is a statement about two roads passing at different heights, so one road cannot be both halves of
// it.
public class GradeSeparatedJunctionUpperEqualsLowerRoadSegment : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.GradeSeparatedJunction.UpperEqualsLowerRoadSegment;

    public GradeSeparatedJunctionUpperEqualsLowerRoadSegment()
        : base(ProblemCode)
    {
    }
}
