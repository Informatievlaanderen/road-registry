namespace RoadRegistry.GradeSeparatedJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record GradeSeparatedJunctionAdded : IMartenEvent, ICreatedEvent
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public GradeSeparatedJunctionId? OriginalId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionType Type { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
