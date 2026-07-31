namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// The road segment has not completed its inwinning: it is not a V2 segment yet and carries no dynamically segmented
// attributes, so it cannot be split or have its attribute values changed.
public class RoadSegmentNotCompletedInwinning : Error
{
    private static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.NotCompletedInwinning;

    public RoadSegmentNotCompletedInwinning(RoadSegmentId identifier)
        : base(ProblemCode.ToString(),
            new ProblemParameter("WegsegmentId", identifier.ToInt32().ToString()))
    {
    }
}
