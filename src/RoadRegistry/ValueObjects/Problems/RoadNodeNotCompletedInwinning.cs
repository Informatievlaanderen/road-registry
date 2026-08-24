namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// The road node has not completed its inwinning: it is not a V2 node yet and carries no type. A V1 node also still
// sits on the coordinate it was imported with, which has more precision than the centimetre the register works in, so
// a segment knotted onto it could never agree with it on where it is.
public class RoadNodeNotCompletedInwinning : Error
{
    private static readonly ProblemCode ProblemCode = ProblemCode.RoadNode.NotCompletedInwinning;

    // For callers that collect their problems under the road node's context, which supplies the identifier.
    public RoadNodeNotCompletedInwinning()
        : base(ProblemCode.ToString())
    {
    }

    // For callers without such a context, e.g. an action that names the road segment rather than the node it ran into.
    public RoadNodeNotCompletedInwinning(RoadNodeId identifier)
        : base(ProblemCode.ToString(),
            new ProblemParameter("WegknoopId", identifier.ToInt32().ToString()))
    {
    }
}
