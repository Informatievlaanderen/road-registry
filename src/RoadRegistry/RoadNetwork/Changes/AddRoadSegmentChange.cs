namespace RoadRegistry.RoadNetwork.Changes;

using RoadRegistry.RoadNetwork;

public sealed record AddRoadSegmentChange(RoadSegmentId RoadSegmentId): IRoadNetworkChange;
