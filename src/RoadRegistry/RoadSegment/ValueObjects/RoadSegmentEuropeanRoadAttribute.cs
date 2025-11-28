namespace RoadRegistry.RoadSegment.ValueObjects;

using RoadRegistry.ValueObjects;

public record struct RoadSegmentEuropeanRoadAttribute(
    AttributeId AttributeId,
    EuropeanRoadNumber Number
);
