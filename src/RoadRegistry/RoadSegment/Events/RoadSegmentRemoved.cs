namespace RoadRegistry.RoadSegment.Events;

using RoadRegistry.ValueObjects;

public record RoadSegmentRemoved
{
    public required RoadSegmentId RoadSegmentId { get; init; }
}
