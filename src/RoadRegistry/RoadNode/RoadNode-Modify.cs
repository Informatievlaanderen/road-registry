namespace RoadRegistry.RoadNode;

using Changes;
using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadNode
{
    public Problems Modify(ModifyRoadNodeChange change)
    {
        var problems = Problems.None;

        Apply(new RoadNodeModified
        {
            RoadNodeId = RoadNodeId,
            Geometry = (change.Geometry ?? Geometry).ToGeometryObject(),
            Type = change.Type ?? Type
        });

        return problems;
    }
}
