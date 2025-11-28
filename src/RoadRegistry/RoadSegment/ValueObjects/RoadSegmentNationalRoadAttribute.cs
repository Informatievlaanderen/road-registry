namespace RoadRegistry.RoadSegment.ValueObjects;

using RoadRegistry.ValueObjects;

public record struct RoadSegmentNationalRoadAttribute(
    AttributeId AttributeId,
    NationalRoadNumber Number
);
