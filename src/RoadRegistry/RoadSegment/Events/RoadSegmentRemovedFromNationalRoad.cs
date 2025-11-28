namespace RoadRegistry.RoadSegment.Events;

using RoadRegistry.ValueObjects;

public record RoadSegmentRemovedFromNationalRoad
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required NationalRoadNumber Number { get; init; }
}
