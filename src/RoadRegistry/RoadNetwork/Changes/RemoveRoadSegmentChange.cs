namespace RoadRegistry.RoadNetwork.Changes;

using RoadSegment.ValueObjects;

public sealed record RemoveRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentId Id { get; init; }
}
