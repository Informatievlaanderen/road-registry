namespace RoadRegistry.Organization.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record OrganizationWasCreated : IMartenEvent
{
    public required OrganizationId OrganizationId { get; init; }
    public required string Name { get; init; }
    public required string OvoCode { get; init; }
    public required string KboNumber { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
