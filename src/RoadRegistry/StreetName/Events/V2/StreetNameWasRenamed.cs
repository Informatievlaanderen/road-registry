namespace RoadRegistry.StreetName.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record StreetNameWasRenamed : IMartenEvent
{
    public required StreetNameLocalId StreetNameId { get; init; }
    public required StreetNameLocalId DestinationStreetNameId { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
