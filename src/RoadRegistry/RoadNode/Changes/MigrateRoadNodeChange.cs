namespace RoadRegistry.RoadNode.Changes;

using ScopedRoadNetwork;

public sealed record MigrateRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId RoadNodeId { get; init; }
    public required RoadNodeGeometry Geometry { get; init; }
    public required RoadNodeType Type { get; init; }
}
