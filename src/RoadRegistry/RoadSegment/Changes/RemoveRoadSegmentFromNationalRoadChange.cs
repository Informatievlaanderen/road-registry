namespace RoadRegistry.RoadSegment.Changes;

using RoadRegistry.ValueObjects;
using ScopedRoadNetwork;

public sealed record RemoveRoadSegmentFromNationalRoadChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required NationalRoadNumber Number { get; init; }
}
