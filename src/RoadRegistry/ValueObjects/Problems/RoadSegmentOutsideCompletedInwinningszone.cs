namespace RoadRegistry.ValueObjects.Problems;

using RoadRegistry.ValueObjects.ProblemCodes;

public class RoadSegmentOutsideCompletedInwinningszone : Error
{
    public RoadSegmentOutsideCompletedInwinningszone()
        : base(ProblemCode.RoadSegment.OutsideCompletedInwinningszone.ToString())
    {
    }
}
