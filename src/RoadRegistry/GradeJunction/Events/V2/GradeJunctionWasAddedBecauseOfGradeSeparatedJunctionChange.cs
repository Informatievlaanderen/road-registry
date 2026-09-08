namespace RoadRegistry.GradeJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// The other half of changing a grade separated junction into a grade one: this junction was not newly observed, it is
// the crossing the named grade separated junction used to record. It carries that identifier so the two halves of the
// action can be tied back together.
public record GradeJunctionWasAddedBecauseOfGradeSeparatedJunctionChange : IMartenEvent, ICreatedEvent
{
    public const string EventName = "GradeJunctionWasAddedBecauseOfGradeSeparatedJunctionChange"; // BE CAREFUL CHANGING THIS!!

    public required GradeJunctionId GradeJunctionId { get; init; }
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public required RoadSegmentId RoadSegmentId1 { get; init; }
    public required RoadSegmentId RoadSegmentId2 { get; init; }
    public required JunctionGeometry Geometry { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
