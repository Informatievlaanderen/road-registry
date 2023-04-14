namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentOutlinedNotFound : Error
{
    public RoadSegmentOutlinedNotFound()
        : base(ProblemCode.RoadSegment.OutlinedNotFound)
    {
    }
}
