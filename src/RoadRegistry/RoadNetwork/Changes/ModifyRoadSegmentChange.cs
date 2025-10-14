namespace RoadRegistry.RoadNetwork.Changes;

using RoadRegistry.RoadNetwork;

public sealed record ModifyRoadSegmentChange(RoadSegmentId RoadSegmentId): IRoadNetworkChange;
