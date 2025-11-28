namespace RoadRegistry.RoadNode.Events;

public record RoadNodeAdded: ICreatedEvent
{
    public required RoadNodeId RoadNodeId { get; init; }
    public RoadNodeId? OriginalId { get; init; }
    public required GeometryObject Geometry { get; init; }
    public required RoadNodeType Type { get; init; }
}
