namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record RoadNodeWasRemoved : IMartenEvent
{
    public required RoadNodeId RoadNodeId { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
