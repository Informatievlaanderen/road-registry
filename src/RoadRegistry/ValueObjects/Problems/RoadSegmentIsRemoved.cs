namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// The road segment is still part of the road network that was loaded, but it has been removed - by an earlier change,
// or by an inwinning of a neighbouring municipality. There is nothing left to act on.
public class RoadSegmentIsRemoved : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.IsRemoved;

    // For callers that collect their problems under the entity context, which supplies the identifier.
    public RoadSegmentIsRemoved()
        : base(ProblemCode.ToString())
    {
    }

    // For callers without such a context.
    public RoadSegmentIsRemoved(RoadSegmentId identifier)
        : base(ProblemCode,
            new ProblemParameter("WegsegmentId", identifier.ToInt32().ToString()))
    {
    }
}
