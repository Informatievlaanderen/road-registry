namespace RoadRegistry.RoadSegment.ValueObjects;

using BackOffice;

public record struct RoadSegmentEuropeanRoadAttribute(
    AttributeId AttributeId,
    EuropeanRoadNumber Number
);
