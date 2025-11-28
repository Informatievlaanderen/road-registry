namespace RoadRegistry.RoadSegment.Events;

using RoadRegistry.ValueObjects;

public record RoadSegmentRemovedFromEuropeanRoad
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required EuropeanRoadNumber Number { get; init; }

    public RoadSegmentRemovedFromEuropeanRoad()
    {
    }
    protected RoadSegmentRemovedFromEuropeanRoad(RoadSegmentRemovedFromEuropeanRoad other) // Needed for Marten
    {
    }
}
