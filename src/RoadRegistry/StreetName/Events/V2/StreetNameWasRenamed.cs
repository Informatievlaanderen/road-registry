namespace RoadRegistry.StreetName.Events.V2;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.BackOffice;
using RoadRegistry.ValueObjects;

public record StreetNameWasRenamed : IMartenEvent
{
    public const string EventName = "StreetNameWasRenamed"; // BE CAREFUL CHANGING THIS!!

    public required StreetNameLocalId StreetNameId { get; init; }
    public required StreetNameLocalId DestinationStreetNameId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
