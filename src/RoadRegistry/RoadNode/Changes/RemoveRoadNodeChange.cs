namespace RoadRegistry.RoadNode.Changes;

using ScopedRoadNetwork;

public sealed record RemoveRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId RoadNodeId { get; init; }
}
