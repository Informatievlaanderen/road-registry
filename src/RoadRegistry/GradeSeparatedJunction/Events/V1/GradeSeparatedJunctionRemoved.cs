namespace RoadRegistry.GradeSeparatedJunction.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class GradeSeparatedJunctionRemoved : IMartenEvent
{
    public const string EventName = "GradeSeparatedJunctionRemoved"; // BE CAREFUL CHANGING THIS!!

    public required int Id { get; set; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
