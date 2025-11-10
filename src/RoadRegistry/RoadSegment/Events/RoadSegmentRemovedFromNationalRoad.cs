namespace RoadRegistry.RoadSegment.Events;

using BackOffice;
using ValueObjects;

public class RoadSegmentRemovedFromNationalRoad
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required NationalRoadNumber Number { get; init; }
}
