namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class LowerRoadSegmentMissing : Error
{
    public LowerRoadSegmentMissing()
        : base(ProblemCode.RoadSegment.LowerMissing)
    {
    }
}
