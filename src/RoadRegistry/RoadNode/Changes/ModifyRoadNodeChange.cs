namespace RoadRegistry.RoadNode.Changes;

using ScopedRoadNetwork;

public sealed record ModifyRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId RoadNodeId { get; init; }
    public RoadNodeGeometry? Geometry { get; init; }
    public RoadNodeType? Type { get; init; }
}
