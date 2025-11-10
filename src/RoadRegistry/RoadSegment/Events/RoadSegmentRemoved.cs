namespace RoadRegistry.RoadSegment.Events;

using ValueObjects;

public record RoadSegmentRemoved
{
    public required RoadSegmentId RoadSegmentId { get; init; }
}
