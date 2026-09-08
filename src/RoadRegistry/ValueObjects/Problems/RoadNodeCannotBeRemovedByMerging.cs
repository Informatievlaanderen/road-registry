namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-4: removing a road node means merging the roads that met there, and only two shapes of node can be undone that
// way - a validatieknoop, which holds exactly the two roads that are to become one, and an echte knoop with exactly
// four, which become two. Anything else is a node that is there for a reason.
public class RoadNodeCannotBeRemovedByMerging : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadNode.Remove.CannotBeRemovedByMerging;

    public RoadNodeCannotBeRemovedByMerging(RoadNodeId identifier)
        : base(ProblemCode,
            new ProblemParameter("WegknoopId", identifier.ToInt32().ToString()))
    {
    }
}
