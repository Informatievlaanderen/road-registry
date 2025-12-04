namespace RoadRegistry.CommandHandling.Actions.RemoveRoadSegments;

public class RemoveRoadSegmentsCommand
{
    public required IReadOnlyCollection<RoadSegmentId> RoadSegmentIds { get; init; }
}
