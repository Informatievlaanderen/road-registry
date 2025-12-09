namespace RoadRegistry.RoadNode.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public class RoadNodeAdded : IMartenEvent
{
    public required RoadNodeGeometry Geometry { get; set; }
    public required int Id { get; set; }
    public required int Version { get; set; }
    public required int TemporaryId { get; set; }
    public required int? OriginalId { get; set; }
    public required string Type { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
