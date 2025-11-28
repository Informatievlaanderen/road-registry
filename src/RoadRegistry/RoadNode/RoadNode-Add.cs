namespace RoadRegistry.RoadNode;

using Changes;
using Events;
using RoadNetwork;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadNode
{
    public static (RoadNode?, Problems) Add(AddRoadNodeChange change, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.None;

        var roadNode = Create(new RoadNodeAdded
        {
            RoadNodeId = idGenerator.NewRoadNodeId(),
            OriginalId = change.OriginalId ?? change.TemporaryId,
            Geometry = change.Geometry.ToGeometryObject(),
            Type = change.Type
        });

        return (roadNode, problems);
    }
}
