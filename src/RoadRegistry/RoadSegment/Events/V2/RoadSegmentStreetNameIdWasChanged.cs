namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public record RoadSegmentStreetNameIdWasChanged : IMartenEvent
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required StreetNameLocalId OldStreetNameId { get; init; }
    public required StreetNameLocalId NewStreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
