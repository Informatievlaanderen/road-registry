namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentMissing : Error
{
    public RoadSegmentMissing(RoadSegmentId segmentId)
        : base(ProblemCode.RoadSegment.Missing,
            new ProblemParameter("SegmentId", segmentId.ToInt32().ToString()))
    {
    }
}
