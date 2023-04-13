namespace RoadRegistry.BackOffice;

public record struct RoadSegmentLaneAttribute(
    RoadSegmentPosition From,
    RoadSegmentPosition To,
    RoadSegmentLaneCount Count,
    RoadSegmentLaneDirection Direction
);
