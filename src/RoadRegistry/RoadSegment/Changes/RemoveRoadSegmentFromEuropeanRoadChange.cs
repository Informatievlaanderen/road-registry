namespace RoadRegistry.RoadSegment.Changes;

using RoadNetwork;
using RoadRegistry.ValueObjects;

public sealed record RemoveRoadSegmentFromEuropeanRoadChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required EuropeanRoadNumber Number { get; init; }
}
