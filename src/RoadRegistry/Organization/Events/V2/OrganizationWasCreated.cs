namespace RoadRegistry.Organization.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record OrganizationWasCreated : IMartenEvent
{
    public required OrganizationId OrganizationId { get; init; }
    public required string Name { get; set; }
    public required string OvoCode { get; set; }
    public required string KboNumber { get; set; }

    public required ProvenanceData Provenance { get; init; }
}
