namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction : Error
{
    public IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(RoadSegmentId modifiedRoadSegmentId, RoadSegmentId intersectingRoadSegmentId)
        : base(ProblemCode.RoadSegment.IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction,
            new ProblemParameter(nameof(modifiedRoadSegmentId), modifiedRoadSegmentId.ToString()),
            new ProblemParameter(nameof(intersectingRoadSegmentId), intersectingRoadSegmentId.ToString()))
    {
    }
}
