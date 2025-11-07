namespace RoadRegistry.RoadSegment.Changes;

using BackOffice;
using RoadNetwork;
using ValueObjects;

public sealed record AddRoadSegmentToNationalRoadChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required NationalRoadNumber Number { get; init; }
}
