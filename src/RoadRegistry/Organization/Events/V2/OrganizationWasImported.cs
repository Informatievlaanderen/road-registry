namespace RoadRegistry.Organization.Events.V2;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.BackOffice;
using RoadRegistry.ValueObjects;

public record OrganizationWasImported : IMartenEvent
{
    public const string EventName = "OrganizationWasImported"; // BE CAREFUL CHANGING THIS!!

    public required OrganizationId OrganizationId { get; init; }
    public required string Name { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
