namespace RoadRegistry.BackOffice;

public record struct RoadSegmentSurfaceAttribute(
    RoadSegmentPosition From,
    RoadSegmentPosition To,
    RoadSegmentSurfaceType Type
);
