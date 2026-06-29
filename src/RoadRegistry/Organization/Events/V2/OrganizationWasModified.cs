namespace RoadRegistry.Organization.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record OrganizationWasModified : IMartenEvent
{
    public const string EventName = "OrganizationWasModified"; // BE CAREFUL CHANGING THIS!!

    public required OrganizationId OrganizationId { get; init; }

    public string? Name { get; init; }
    public string? OvoCode { get; init; }
    public string? KboNumber { get; init; }
    public bool? IsMaintainer { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
