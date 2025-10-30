namespace RoadRegistry.RoadNetwork.Changes;

using BackOffice;
using RoadSegment.ValueObjects;

public sealed record AddGradeSeparatedJunctionChange : IRoadNetworkChange
{
    public required GradeSeparatedJunctionId TemporaryId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionType Type { get; init; }
}
