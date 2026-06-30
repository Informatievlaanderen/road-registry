namespace RoadRegistry.Organization.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record OrganizationWasImported : IMartenEvent
{
    public required OrganizationId OrganizationId { get; init; }
    public required string Name { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
