namespace RoadRegistry.GradeSeparatedJunction.Events.V1;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class ImportedGradeSeparatedJunction : IMartenEvent
{
    public const string EventName = "ImportedGradeSeparatedJunction"; // BE CAREFUL CHANGING THIS!!

    public required int Id { get; set; }
    public required int LowerRoadSegmentId { get; set; }
    public required ImportedOriginProperties Origin { get; set; }
    public required string Type { get; set; }
    public required int UpperRoadSegmentId { get; set; }
    public required DateTimeOffset When {get; set; }

    public required ProvenanceData Provenance { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
