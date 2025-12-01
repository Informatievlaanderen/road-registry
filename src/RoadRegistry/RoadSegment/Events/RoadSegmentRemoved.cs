namespace RoadRegistry.RoadSegment.Events;

using RoadRegistry.ValueObjects;

public record RoadSegmentRemoved
{
    public required RoadSegmentId RoadSegmentId { get; init; }

    public RoadSegmentRemoved()
    {
    }
    protected RoadSegmentRemoved(RoadSegmentRemoved other) // Needed for Marten
    {
    }
}
