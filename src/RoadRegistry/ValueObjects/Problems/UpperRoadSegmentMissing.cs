namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class UpperRoadSegmentMissing : Error
{
    public UpperRoadSegmentMissing()
        : base(ProblemCode.RoadSegment.UpperMissing)
    {
    }
}
