namespace RoadRegistry.RoadNode.Events;

public record RoadNodeRemoved
{
    public required RoadNodeId RoadNodeId { get; init; }

    public RoadNodeRemoved()
    {
    }
    protected RoadNodeRemoved(RoadNodeRemoved other) // Needed for Marten
    {
    }
}
