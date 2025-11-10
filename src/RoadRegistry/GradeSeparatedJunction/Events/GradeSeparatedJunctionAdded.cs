namespace RoadRegistry.GradeSeparatedJunction.Events;

using BackOffice;
using RoadSegment.ValueObjects;

public class GradeSeparatedJunctionAdded : ICreatedEvent
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public required GradeSeparatedJunctionId TemporaryId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionType Type { get; init; }
}
