namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentStatusNotValid : Error
{
    public RoadSegmentStatusNotValid(RoadSegmentStatus status)
        : base(ProblemCode.RoadSegment.Status.NotValid,
        new ProblemParameter("Status", status))
    {
    }
}
