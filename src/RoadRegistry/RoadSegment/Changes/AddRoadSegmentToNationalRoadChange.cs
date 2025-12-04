namespace RoadRegistry.RoadSegment.Changes;

using RoadNetwork;
using RoadRegistry.ValueObjects;

public sealed record AddRoadSegmentToNationalRoadChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required NationalRoadNumber Number { get; init; }
}
