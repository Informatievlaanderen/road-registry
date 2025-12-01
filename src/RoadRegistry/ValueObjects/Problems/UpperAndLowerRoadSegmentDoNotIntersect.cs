namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class UpperAndLowerRoadSegmentDoNotIntersect : Error
{
    public UpperAndLowerRoadSegmentDoNotIntersect()
        : base(ProblemCode.RoadSegment.UpperAndLowerDoNotIntersect)
    {
    }
}
