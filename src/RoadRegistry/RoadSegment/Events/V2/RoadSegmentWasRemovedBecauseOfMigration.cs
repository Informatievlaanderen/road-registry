namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record RoadSegmentWasRemovedBecauseOfMigration: IMartenEvent
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required RoadNodeId StartNodeId { get; init; }
    public required RoadNodeId EndNodeId { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
