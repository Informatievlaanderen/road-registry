namespace RoadRegistry.RoadSegment.Events;

using ValueObjects;

public class RoadSegmentRemoved
{
    public required RoadSegmentId RoadSegmentId { get; init; }
}
