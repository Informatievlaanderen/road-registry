namespace RoadRegistry.RoadSegment.Events;

using RoadRegistry.ValueObjects;

public record RoadSegmentAddedToEuropeanRoad
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required EuropeanRoadNumber Number { get; init; }
}
