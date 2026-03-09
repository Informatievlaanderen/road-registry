namespace RoadRegistry.ValueObjects.Problems;

using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects.ProblemCodes;

public class RoadSegmentDuplicateIntersections : Error
{
    public RoadSegmentDuplicateIntersections(RoadSegmentIdReference intersectingRoadSegmentId)
        : base(ProblemCode.RoadSegment.DuplicateIntersections.ToString(),
            intersectingRoadSegmentId.ToRoadSegmentProblemParameters("IntersectingWegsegment").ToArray())
    {
    }
}
