namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-4: only a road segment with status 'gepland' can be realized. Which road segment it concerns comes from the
// problem context.
public class RoadSegmentRealizeStatusNotValid : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Realize.StatusNotValid;

    public RoadSegmentRealizeStatusNotValid()
        : base(ProblemCode.ToString())
    {
    }
}
