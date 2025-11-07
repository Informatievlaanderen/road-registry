namespace RoadRegistry.RoadSegment.Changes;

using BackOffice;
using RoadNetwork;
using ValueObjects;

public sealed record RemoveRoadSegmentFromEuropeanRoadChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required EuropeanRoadNumber Number { get; init; }
}
