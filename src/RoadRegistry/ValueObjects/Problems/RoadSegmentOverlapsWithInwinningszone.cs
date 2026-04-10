namespace RoadRegistry.ValueObjects.Problems;

using RoadRegistry.ValueObjects.ProblemCodes;

public class RoadSegmentOverlapsWithInwinningszone : Error
{
    public RoadSegmentOverlapsWithInwinningszone()
        : base(ProblemCode.RoadSegment.OverlapsWithInwinningszone.ToString())
    {
    }
}
