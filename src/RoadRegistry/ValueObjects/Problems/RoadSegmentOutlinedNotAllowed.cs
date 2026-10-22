namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// Only a holder of the 'geschetste weg' scope may edit an ingeschetst road segment. The identifier of the road
// segment comes from the problem context.
public class RoadSegmentOutlinedNotAllowed : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.OutlinedNotAllowed;

    public RoadSegmentOutlinedNotAllowed()
        : base(ProblemCode.ToString())
    {
    }
}
