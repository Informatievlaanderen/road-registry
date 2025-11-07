namespace RoadRegistry.RoadNode.Changes;

using RoadNetwork;
using RoadRegistry.BackOffice;

public sealed record RemoveRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId Id { get; init; }
}
