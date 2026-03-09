namespace RoadRegistry.ValueObjects.Problems;

using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects.ProblemCodes;

public class IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction : Error
{
    public IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(RoadSegmentIdReference intersectingRoadSegmentId)
        : base(ProblemCode.RoadSegment.IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction.ToString(),
            intersectingRoadSegmentId.ToRoadSegmentProblemParameters("IntersectingWegsegment").ToArray())
    {
    }

    public IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(RoadSegmentId modifiedRoadSegmentId, RoadSegmentId intersectingRoadSegmentId)
        : base(ProblemCode.RoadSegment.IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction,
            new ProblemParameter("ModifiedRoadSegmentId", modifiedRoadSegmentId.ToString()),
            new ProblemParameter("IntersectingRoadSegmentId", intersectingRoadSegmentId.ToString()))
    {
    }
}
