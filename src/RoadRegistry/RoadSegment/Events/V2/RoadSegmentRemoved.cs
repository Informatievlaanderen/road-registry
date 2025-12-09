namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record RoadSegmentRemoved : IMartenEvent
{
    public required RoadSegmentId RoadSegmentId { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
