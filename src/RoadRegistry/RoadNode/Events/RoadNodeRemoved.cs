namespace RoadRegistry.RoadNode.Events;

using BackOffice;

public record RoadNodeRemoved
{
    public required RoadNodeId RoadNodeId { get; init; }
}
