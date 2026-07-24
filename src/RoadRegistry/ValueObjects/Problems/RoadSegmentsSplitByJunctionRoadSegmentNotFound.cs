namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-2: the given value must match the objectidentificator of an existing, non-removed road segment.
public class RoadSegmentsSplitByJunctionRoadSegmentNotFound : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.SplitByJunction.RoadSegmentNotFound;

    public RoadSegmentsSplitByJunctionRoadSegmentNotFound(RoadSegmentId identifier)
        : base(ProblemCode,
            new ProblemParameter("WegsegmentId", identifier.ToInt32().ToString()))
    {
    }
}
