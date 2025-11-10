namespace RoadRegistry.RoadNode;

using BackOffice.Core;
using Changes;
using Events;

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
