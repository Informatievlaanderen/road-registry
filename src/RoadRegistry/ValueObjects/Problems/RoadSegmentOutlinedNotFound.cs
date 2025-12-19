namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentOutlinedNotFound : Error
{
    public RoadSegmentOutlinedNotFound()
        : base(ProblemCode.RoadSegment.OutlinedNotFound)
    {
    }
}
