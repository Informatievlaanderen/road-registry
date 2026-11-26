namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// The road segment let go of a street name: wherever it carried StreetNameId it now carries 'niet van toepassing'.
public record RoadSegmentStreetNameWasUnlinked : IMartenEvent
{
    public const string EventName = "RoadSegmentStreetNameWasUnlinked"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }
    public required StreetNameLocalId StreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameIdAttributeValues { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
