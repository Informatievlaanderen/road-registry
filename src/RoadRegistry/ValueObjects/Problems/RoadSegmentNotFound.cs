namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentNotFound : Error
{
    public RoadSegmentNotFound()
        : base(ProblemCode.RoadSegment.NotFound.ToString())
    {
    }

    public RoadSegmentNotFound(RoadSegmentId segmentId)
        : base(ProblemCode.RoadSegment.NotFound,
            new ProblemParameter("SegmentId", segmentId.ToInt32().ToString()))
    {
    }
}
