namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record RoadNodeWasMigrated : IMartenEvent
{
    public required RoadNodeId RoadNodeId { get; init; }
    public required RoadNodeGeometry Geometry { get; init; }
    public required RoadNodeTypeV2 Type { get; init; }
    public required bool Grensknoop { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
