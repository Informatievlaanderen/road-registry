namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentMissing : Error
{
    public RoadSegmentMissing(RoadSegmentId segmentId)
        : base(ProblemCode.RoadSegment.Missing,
            new ProblemParameter("SegmentId", segmentId.ToInt32().ToString()))
    {
    }
}
