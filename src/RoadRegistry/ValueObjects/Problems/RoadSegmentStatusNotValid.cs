namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentStatusNotValid : Error
{
    public RoadSegmentStatusNotValid(RoadSegmentStatus status)
        : base(ProblemCode.RoadSegment.Status.NotValid,
        new ProblemParameter("Status", status))
    {
    }

    public RoadSegmentStatusNotValid(RoadSegmentStatusV2 status)
        : base(ProblemCode.RoadSegment.Status.NotValid,
            new ProblemParameter("Status", status))
    {
    }
}
