namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record RoadNodeWasAdded: IMartenEvent, ICreatedEvent
{
    public required RoadNodeId RoadNodeId { get; init; }
    public RoadNodeId? OriginalId { get; init; }
    public required RoadNodeGeometry Geometry { get; init; }
    public required bool Grensknoop { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
