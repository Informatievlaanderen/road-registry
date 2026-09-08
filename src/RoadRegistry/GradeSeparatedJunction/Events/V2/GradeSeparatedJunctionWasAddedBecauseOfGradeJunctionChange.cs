namespace RoadRegistry.GradeSeparatedJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// The other half of changing a grade junction into a grade separated one: this junction was not newly observed, it is
// the crossing the named grade junction used to record. It carries that identifier so the two halves of the action can
// be tied back together.
public record GradeSeparatedJunctionWasAddedBecauseOfGradeJunctionChange : IMartenEvent, ICreatedEvent
{
    public const string EventName = "GradeSeparatedJunctionWasAddedBecauseOfGradeJunctionChange"; // BE CAREFUL CHANGING THIS!!

    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public required GradeJunctionId GradeJunctionId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionTypeV2 Type { get; init; }
    public required JunctionGeometry Geometry { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
