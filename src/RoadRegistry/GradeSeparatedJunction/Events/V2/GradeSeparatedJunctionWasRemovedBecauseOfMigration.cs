namespace RoadRegistry.GradeSeparatedJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record GradeSeparatedJunctionWasRemovedBecauseOfMigration : IMartenEvent
{
    public const string EventName = "GradeSeparatedJunctionWasRemovedBecauseOfMigration"; // BE CAREFUL CHANGING THIS!!

    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public RoadSegmentId LowerRoadSegmentId { get; init; }
    public RoadSegmentId UpperRoadSegmentId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
