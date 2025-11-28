namespace RoadRegistry.RoadNode.Events;

public record RoadNodeRemoved
{
    public required RoadNodeId RoadNodeId { get; init; }
}
