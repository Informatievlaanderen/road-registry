namespace RoadRegistry.Organization.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record OrganizationWasModified : IMartenEvent
{
    public required OrganizationId OrganizationId { get; init; }

    public string? Name { get; init; }
    public string? OvoCode { get; init; }
    public string? KboNumber { get; init; }
    public bool? IsMaintainer { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
