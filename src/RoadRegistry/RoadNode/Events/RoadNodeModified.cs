namespace RoadRegistry.RoadNode.Events;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record RoadNodeModified : IMartenEvent
{
    public required RoadNodeId RoadNodeId { get; init; }
    public GeometryObject? Geometry { get; init; }
    public RoadNodeType? Type { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public RoadNodeModified()
    {
    }
    protected RoadNodeModified(RoadNodeModified other) // Needed for Marten
    {
    }
}
