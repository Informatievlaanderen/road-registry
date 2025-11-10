namespace RoadRegistry.RoadSegment.Events;

using BackOffice;
using ValueObjects;

public class RoadSegmentRemovedFromEuropeanRoad
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required EuropeanRoadNumber Number { get; init; }
}
