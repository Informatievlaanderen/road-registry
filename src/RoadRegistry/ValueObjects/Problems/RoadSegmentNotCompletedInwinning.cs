namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// The road segment has not completed its inwinning: it is not a V2 segment yet and carries no dynamically segmented
// attributes, so it cannot be split or have its attribute values changed.
public class RoadSegmentNotCompletedInwinning : Error
{
    private static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.NotCompletedInwinning;

    // For callers that collect their problems under the road segment's context, which supplies the identifier.
    public RoadSegmentNotCompletedInwinning()
        : base(ProblemCode.ToString())
    {
    }

    // For callers without such a context, e.g. the API request validators.
    public RoadSegmentNotCompletedInwinning(RoadSegmentId identifier)
        : base(ProblemCode.ToString(),
            new ProblemParameter("WegsegmentId", identifier.ToInt32().ToString()))
    {
    }
}
