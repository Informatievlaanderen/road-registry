namespace RoadRegistry.RoadNode.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public class RoadNodeModified : IMartenEvent
{
    public required RoadNodeGeometry Geometry { get; set; }
    public required int RoadNodeId { get; set; }
    public required int Version { get; set; }
    public required string Type { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
