namespace RoadRegistry.GradeSeparatedJunction.Events.V1;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public class ImportedGradeSeparatedJunction : IMartenEvent
{
    public required int Id { get; set; }
    public required int LowerRoadSegmentId { get; set; }
    public required ImportedOriginProperties Origin { get; set; }
    public required string Type { get; set; }
    public required int UpperRoadSegmentId { get; set; }
    public required DateTimeOffset When {get; set; }

    public required ProvenanceData Provenance { get; set; }
}
