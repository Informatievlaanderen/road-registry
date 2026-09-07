namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-4: an identifier in the request that no road node carries.
public class RoadNodeChangeAttributesNotFound : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadNode.ChangeAttributes.NotFound;

    public RoadNodeChangeAttributesNotFound(RoadNodeId identifier)
        : base(ProblemCode,
            new ProblemParameter("WegknoopId", identifier.ToInt32().ToString()))
    {
    }
}
