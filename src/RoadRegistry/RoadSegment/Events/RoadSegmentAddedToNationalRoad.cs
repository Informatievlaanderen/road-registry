namespace RoadRegistry.RoadSegment.Events;

using BackOffice;
using ValueObjects;

public record RoadSegmentAddedToNationalRoad
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required NationalRoadNumber Number { get; init; }
}
