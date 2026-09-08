namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-3: the road node is still known but has been removed, so there is nothing left to remove.
public class RoadNodeRemoveIsRemoved : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadNode.Remove.IsRemoved;

    public RoadNodeRemoveIsRemoved(RoadNodeId identifier)
        : base(ProblemCode,
            new ProblemParameter("WegknoopId", identifier.ToInt32().ToString()))
    {
    }
}
