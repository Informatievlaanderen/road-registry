namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// Only a holder of the 'geschetste weg' scope may edit an ingeschetst road segment. Which road segment is ingeschetst
// is not necessarily the one the request names - a connected segment dragged along by the change counts just the same.
public class RoadSegmentOutlinedNotAllowed : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.OutlinedNotAllowed;

    // For callers that collect their problems under the road segment's context, which supplies the identifier.
    public RoadSegmentOutlinedNotAllowed()
        : base(ProblemCode.ToString())
    {
    }

    // For callers without such a context.
    public RoadSegmentOutlinedNotAllowed(RoadSegmentId identifier)
        : base(ProblemCode.ToString(),
            new ProblemParameter("WegsegmentId", identifier.ToInt32().ToString()))
    {
    }
}
