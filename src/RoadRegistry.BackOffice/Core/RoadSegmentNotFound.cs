namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentNotFound : Error
{
    public RoadSegmentNotFound()
        : base(ProblemCode.RoadSegment.NotFound)
    {
    }
}
