namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// Only a holder of the 'ingemeten weg' scope may edit an ingemeten road segment. Which road segment is ingemeten is
// not necessarily the one the request names - a connected segment dragged along by the change counts just the same.
public class RoadSegmentMeasuredNotAllowed : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.MeasuredNotAllowed;

    // For callers that collect their problems under the road segment's context, which supplies the identifier.
    public RoadSegmentMeasuredNotAllowed()
        : base(ProblemCode.ToString())
    {
    }

    // For callers without such a context.
    public RoadSegmentMeasuredNotAllowed(RoadSegmentId identifier)
        : base(ProblemCode.ToString(),
            new ProblemParameter("WegsegmentId", identifier.ToInt32().ToString()))
    {
    }
}
