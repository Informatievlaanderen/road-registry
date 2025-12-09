namespace RoadRegistry.RoadNode.Events.V1;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public class ImportedRoadNode : IMartenEvent
{
    public required RoadNodeGeometry Geometry { get; set; }
    public required int Id { get; set; }
    public required ImportedOriginProperties Origin { get; set; }
    public required string Type { get; set; }
    public required int Version { get; set; }
    public required DateTimeOffset When {get; set; }

    public required ProvenanceData Provenance { get; set; }
}
