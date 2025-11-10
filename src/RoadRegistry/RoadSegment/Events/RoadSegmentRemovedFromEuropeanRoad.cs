namespace RoadRegistry.RoadSegment.Events;

using BackOffice;
using ValueObjects;

public record RoadSegmentRemovedFromEuropeanRoad
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required EuropeanRoadNumber Number { get; init; }
}
