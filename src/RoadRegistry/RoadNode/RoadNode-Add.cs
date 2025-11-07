namespace RoadRegistry.RoadNode;

using BackOffice.Core;
using Changes;
using Events;
using RoadNetwork.ValueObjects;

public partial class RoadNode
{
    public static (RoadNode?, Problems) Add(AddRoadNodeChange change, RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        var roadNode = Create(new RoadNodeAdded
        {
            Id = context.IdGenerator.NewRoadNodeId(),
            TemporaryId = change.TemporaryId,
            OriginalId = change.OriginalId,
            Geometry = change.Geometry.ToGeometryObject(),
            Type = change.Type
        });

        return (roadNode, problems);
    }
}
