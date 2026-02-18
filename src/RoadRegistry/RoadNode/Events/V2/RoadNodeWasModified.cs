namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record RoadNodeWasModified : IMartenEvent
{
    public required RoadNodeId RoadNodeId { get; init; }
    public RoadNodeGeometry? Geometry { get; init; }
    public bool? Grensknoop { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
