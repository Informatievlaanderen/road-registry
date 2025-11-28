namespace RoadRegistry.BackOffice.Core
{
    using RoadRegistry.RoadNetwork.ValueObjects;

    public readonly record struct RoadSegmentLane(RoadSegmentLaneCount Count, RoadSegmentLaneDirection Direction) { }
}
