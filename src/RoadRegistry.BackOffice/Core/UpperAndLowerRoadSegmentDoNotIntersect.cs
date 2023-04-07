namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class UpperAndLowerRoadSegmentDoNotIntersect : Error
{
    public UpperAndLowerRoadSegmentDoNotIntersect()
        : base(ProblemCode.RoadSegment.UpperAndLowerDoNotIntersect)
    {
    }
}
