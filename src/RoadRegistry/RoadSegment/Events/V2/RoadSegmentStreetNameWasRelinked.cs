namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// The road segment follows a street name to another one, a renamed street name for instance: wherever it carried
// OldStreetNameId it now carries NewStreetNameId.
public record RoadSegmentStreetNameWasRelinked : IMartenEvent
{
    public const string EventName = "RoadSegmentStreetNameWasRelinked"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }
    public required StreetNameLocalId OldStreetNameId { get; init; }
    public required StreetNameLocalId NewStreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameIdAttributeValues { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
