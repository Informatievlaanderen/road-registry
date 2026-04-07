namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record RoadNodeWasRemovedBecauseOfMigration : IMartenEvent
{
    public required RoadNodeId RoadNodeId { get; init; }
    public required RoadNodeGeometry Geometry { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
