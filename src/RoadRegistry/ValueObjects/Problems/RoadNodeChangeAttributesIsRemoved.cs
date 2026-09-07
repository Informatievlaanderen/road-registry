namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-5: the road node is still known but has been removed, so there is nothing left to change on it.
public class RoadNodeChangeAttributesIsRemoved : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadNode.ChangeAttributes.IsRemoved;

    public RoadNodeChangeAttributesIsRemoved(RoadNodeId identifier)
        : base(ProblemCode,
            new ProblemParameter("WegknoopId", identifier.ToInt32().ToString()))
    {
    }
}
