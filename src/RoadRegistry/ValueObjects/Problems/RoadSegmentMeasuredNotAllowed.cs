namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// Only a holder of the 'ingemeten weg' scope may edit an ingemeten road segment. The identifier of the road segment
// comes from the problem context.
public class RoadSegmentMeasuredNotAllowed : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.MeasuredNotAllowed;

    public RoadSegmentMeasuredNotAllowed()
        : base(ProblemCode.ToString())
    {
    }
}
