namespace RoadRegistry.GradeSeparatedJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record GradeSeparatedJunctionWasAdded : IMartenEvent, ICreatedEvent
{
    public const string EventName = "GradeSeparatedJunctionWasAdded"; // BE CAREFUL CHANGING THIS!!

    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public GradeSeparatedJunctionId? OriginalId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionTypeV2 Type { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
