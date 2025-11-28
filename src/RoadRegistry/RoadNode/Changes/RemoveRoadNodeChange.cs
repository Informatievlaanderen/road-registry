namespace RoadRegistry.RoadNode.Changes;

using RoadNetwork;

public sealed record RemoveRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId RoadNodeId { get; init; }
}
