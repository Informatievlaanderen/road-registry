namespace RoadRegistry.GradeSeparatedJunction.Events;

public record GradeSeparatedJunctionAdded : ICreatedEvent
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public GradeSeparatedJunctionId? OriginalId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionType Type { get; init; }
}
