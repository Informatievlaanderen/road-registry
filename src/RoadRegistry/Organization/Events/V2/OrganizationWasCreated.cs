namespace RoadRegistry.Organization.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record OrganizationWasCreated : IMartenEvent
{
    public const string EventName = "OrganizationWasCreated"; // BE CAREFUL CHANGING THIS!!

    public required OrganizationId OrganizationId { get; init; }
    public required string Name { get; init; }
    public required string OvoCode { get; init; }
    public required string? KboNumber { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
