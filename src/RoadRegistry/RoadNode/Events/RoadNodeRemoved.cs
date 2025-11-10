namespace RoadRegistry.RoadNode.Events;

using BackOffice;

public class RoadNodeRemoved
{
    public required RoadNodeId Id { get; init; }
}
