namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class LowerRoadSegmentMissing : Error
{
    public LowerRoadSegmentMissing()
        : base(ProblemCode.RoadSegment.LowerMissing)
    {
    }
}
