namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record RoadNodeModified : IMartenEvent
{
    public required RoadNodeId RoadNodeId { get; init; }
    public RoadNodeGeometry? Geometry { get; init; }
    public RoadNodeType? Type { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
