namespace RoadRegistry.GradeSeparatedJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record GradeSeparatedJunctionWasRemoved : IMartenEvent
{
    public const string EventName = "GradeSeparatedJunctionWasRemoved"; // BE CAREFUL CHANGING THIS!!

    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
