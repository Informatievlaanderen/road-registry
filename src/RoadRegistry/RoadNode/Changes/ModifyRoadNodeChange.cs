namespace RoadRegistry.RoadNode.Changes;

using ScopedRoadNetwork;

public sealed record ModifyRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId RoadNodeId { get; init; }
    public RoadNodeGeometry? Geometry { get; init; }
    public RoadNodeTypeV2? Type { get; init; }
    public bool? Grensknoop { get; init; }
}
