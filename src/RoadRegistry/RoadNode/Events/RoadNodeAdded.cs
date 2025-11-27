namespace RoadRegistry.RoadNode.Events;

using BackOffice;
using RoadNetwork.ValueObjects;

public record RoadNodeAdded: ICreatedEvent
{
    public required RoadNodeId RoadNodeId { get; init; }
    public required RoadNodeId? OriginalId { get; init; }
    public required GeometryObject Geometry { get; init; }
    public required RoadNodeType Type { get; init; }
}
