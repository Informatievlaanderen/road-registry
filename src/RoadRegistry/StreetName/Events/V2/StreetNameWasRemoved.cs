namespace RoadRegistry.StreetName.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record StreetNameWasRemoved : IMartenEvent
{
    public const string EventName = "StreetNameWasRemoved"; // BE CAREFUL CHANGING THIS!!

    public required StreetNameLocalId StreetNameId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
