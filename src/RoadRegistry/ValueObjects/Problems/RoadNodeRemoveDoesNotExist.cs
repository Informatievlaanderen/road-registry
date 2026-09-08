namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-2 of 'verwijder wegknoop': the identifier names no road node at all.
public class RoadNodeRemoveDoesNotExist : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadNode.Remove.DoesNotExist;

    public RoadNodeRemoveDoesNotExist(RoadNodeId identifier)
        : base(ProblemCode,
            new ProblemParameter("WegknoopId", identifier.ToInt32().ToString()))
    {
    }
}
