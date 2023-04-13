namespace RoadRegistry.BackOffice;

public record struct RoadSegmentWidthAttribute(
    RoadSegmentPosition From,
    RoadSegmentPosition To,
    RoadSegmentWidth Width
);
