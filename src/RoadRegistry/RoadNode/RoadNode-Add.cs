namespace RoadRegistry.RoadNode;

using BackOffice.Core;
using Changes;
using Events;

public partial class RoadNode
{
    public static (RoadNode?, Problems) Add(AddRoadNodeChange change, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.None;

        var roadNode = Create(new RoadNodeAdded
        {
            RoadNodeId = idGenerator.NewRoadNodeId(),
            TemporaryId = change.TemporaryId,
            OriginalId = change.OriginalId,
            Geometry = change.Geometry.ToGeometryObject(),
            Type = change.Type
        });

        return (roadNode, problems);
    }
}
