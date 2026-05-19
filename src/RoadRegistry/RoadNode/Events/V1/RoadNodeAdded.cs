namespace RoadRegistry.RoadNode.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public class RoadNodeAdded : IMartenEvent
{
    public RoadNodeGeometry Geometry { get; set; }
    public int RoadNodeId { get; set; }
    public int Version { get; set; }
    public int TemporaryId { get; set; }
    public int? OriginalId { get; set; }
    public string Type { get; set; }

    public ProvenanceData Provenance { get; set; }
}
