namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentStatusNotValid : Error
{
    public RoadSegmentStatusNotValid(RoadSegmentStatus status)
        : base(ProblemCode.RoadSegment.Status.NotValid,
        new ProblemParameter("Status", status))
    {
    }
}
