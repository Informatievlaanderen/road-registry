namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-3: the given value must match the objectidentificator of a road segment with status 'gerealiseerd'.
public class RoadSegmentsSplitByJunctionStatusNotValid : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.SplitByJunction.StatusNotValid;

    public RoadSegmentsSplitByJunctionStatusNotValid(RoadSegmentId identifier)
        : base(ProblemCode,
            new ProblemParameter("WegsegmentId", identifier.ToInt32().ToString()))
    {
    }
}
