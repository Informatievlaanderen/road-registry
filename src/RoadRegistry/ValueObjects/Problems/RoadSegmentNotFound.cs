namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentNotFound : Error
{
    public RoadSegmentNotFound()
        : base(ProblemCode.RoadSegment.NotFound)
    {
    }

    public RoadSegmentNotFound(RoadSegmentId segmentId)
        : base(ProblemCode.RoadSegment.NotFound,
            new ProblemParameter("SegmentId", segmentId.ToInt32().ToString()))
    {
    }
}
