namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record RoadNodeAdded: IMartenEvent, ICreatedEvent
{
    public required RoadNodeId RoadNodeId { get; init; }
    public RoadNodeId? OriginalId { get; init; }
    public required RoadNodeGeometry Geometry { get; init; }
    public required RoadNodeType Type { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
