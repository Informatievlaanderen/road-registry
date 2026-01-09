namespace RoadRegistry.RoadSegment.Changes;

using RoadRegistry.ValueObjects;
using ScopedRoadNetwork;

public sealed record RemoveRoadSegmentFromEuropeanRoadChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required EuropeanRoadNumber Number { get; init; }
}
