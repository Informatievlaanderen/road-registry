namespace RoadRegistry.GradeSeparatedJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// The mirror of GradeJunctionWasChangedToGradeSeparatedJunction: the bridge or tunnel is gone from the terrain, or was
// recorded in error, and the crossing is at grade after all. An event of its own rather than a plain removal, so a
// reader of the stream can follow the crossing to the junction that took its place.
public record GradeSeparatedJunctionWasChangedToGradeJunction : IMartenEvent
{
    public const string EventName = "GradeSeparatedJunctionWasChangedToGradeJunction"; // BE CAREFUL CHANGING THIS!!

    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public required GradeJunctionId GradeJunctionId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
