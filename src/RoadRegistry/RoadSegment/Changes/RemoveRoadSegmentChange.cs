namespace RoadRegistry.RoadSegment.Changes;

using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;
using ScopedRoadNetwork;

public sealed record RemoveRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
}
