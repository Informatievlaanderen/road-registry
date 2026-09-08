namespace RoadRegistry.GradeJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// The grade junction is gone, but not in the way a removal means it: the crossing it recorded is still there and is
// now recorded as a grade separated junction instead. An event of its own rather than a plain removal, so a reader of
// the stream can tell the two apart and follow the crossing to the junction that took its place.
public record GradeJunctionWasChangedToGradeSeparatedJunction : IMartenEvent
{
    public const string EventName = "GradeJunctionWasChangedToGradeSeparatedJunction"; // BE CAREFUL CHANGING THIS!!

    public required GradeJunctionId GradeJunctionId { get; init; }
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
