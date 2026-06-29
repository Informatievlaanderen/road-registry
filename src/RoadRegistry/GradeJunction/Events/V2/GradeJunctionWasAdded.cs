namespace RoadRegistry.GradeJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record GradeJunctionWasAdded : IMartenEvent, ICreatedEvent
{
    public const string EventName = "GradeJunctionWasAdded"; // BE CAREFUL CHANGING THIS!!

    public required GradeJunctionId GradeJunctionId { get; init; }
    public required RoadSegmentId RoadSegmentId1 { get; init; }
    public required RoadSegmentId RoadSegmentId2 { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
