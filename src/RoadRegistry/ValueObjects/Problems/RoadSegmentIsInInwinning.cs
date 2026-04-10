namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentIsInInwinning : Error
{
    public RoadSegmentIsInInwinning()
        : base(ProblemCode.RoadSegment.IsInInwinning.ToString())
    {
    }
}
