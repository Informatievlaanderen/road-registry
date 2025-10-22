namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction : Error
{
    public IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(RoadSegmentId modifiedRoadSegmentId, RoadSegmentId intersectingRoadSegmentId)
        : base(ProblemCode.RoadSegment.IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction,
            new ProblemParameter("ModifiedRoadSegmentId", modifiedRoadSegmentId.ToString()),
            new ProblemParameter("IntersectingRoadSegmentId", intersectingRoadSegmentId.ToString()))
    {
    }
}
