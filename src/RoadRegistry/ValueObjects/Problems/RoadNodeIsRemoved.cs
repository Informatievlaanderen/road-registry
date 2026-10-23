namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// The road node is still part of the road network that was loaded, but it has been removed - by an earlier change, or
// by an inwinning of a neighbouring municipality. There is nothing left to act on.
public class RoadNodeIsRemoved : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadNode.IsRemoved;

    // For callers that collect their problems under the entity context, which supplies the identifier.
    public RoadNodeIsRemoved()
        : base(ProblemCode.ToString())
    {
    }

    // For callers without such a context.
    public RoadNodeIsRemoved(RoadNodeId identifier)
        : base(ProblemCode,
            new ProblemParameter("WegknoopId", identifier.ToInt32().ToString()))
    {
    }
}
