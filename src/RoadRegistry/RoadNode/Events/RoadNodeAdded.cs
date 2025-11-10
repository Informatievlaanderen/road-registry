namespace RoadRegistry.RoadNode.Events;

using BackOffice;
using RoadNetwork.ValueObjects;

public class RoadNodeAdded: ICreatedEvent
{
    public required RoadNodeId Id { get; init; }
    public required RoadNodeId TemporaryId { get; init; }
    public RoadNodeId? OriginalId { get; init; }
    public required GeometryObject Geometry { get; init; }
    public required RoadNodeType Type { get; init; }
}
