namespace RoadRegistry.StreetName.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record StreetNameWasCreated : IMartenEvent
{
    public required StreetNameLocalId StreetNameId { get; init; }
    public required string? DutchName { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
