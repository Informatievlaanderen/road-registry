namespace RoadRegistry.Organization.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record OrganizationWasRemoved : IMartenEvent
{
    public required OrganizationId OrganizationId { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
