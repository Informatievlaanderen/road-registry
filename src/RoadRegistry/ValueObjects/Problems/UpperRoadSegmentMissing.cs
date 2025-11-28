namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class UpperRoadSegmentMissing : Error
{
    public UpperRoadSegmentMissing()
        : base(ProblemCode.RoadSegment.UpperMissing)
    {
    }
}
