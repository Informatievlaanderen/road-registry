namespace RoadRegistry.RoadSegment.Events;

using RoadRegistry.ValueObjects;

public record RoadSegmentAddedToNationalRoad
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required NationalRoadNumber Number { get; init; }

    public RoadSegmentAddedToNationalRoad()
    {
    }
    protected RoadSegmentAddedToNationalRoad(RoadSegmentAddedToNationalRoad other) // Needed for Marten
    {
    }
}
