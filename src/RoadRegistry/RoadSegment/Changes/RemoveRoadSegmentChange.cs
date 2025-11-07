namespace RoadRegistry.RoadSegment.Changes;

using RoadNetwork;
using RoadRegistry.RoadSegment.ValueObjects;

public sealed record RemoveRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
}
