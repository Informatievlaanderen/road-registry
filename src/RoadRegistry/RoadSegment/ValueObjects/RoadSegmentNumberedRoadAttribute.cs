namespace RoadRegistry.RoadSegment.ValueObjects;

using RoadRegistry.ValueObjects;

public record struct RoadSegmentNumberedRoadAttribute(
    AttributeId AttributeId,
    RoadSegmentNumberedRoadDirection Direction,
    NumberedRoadNumber Number,
    RoadSegmentNumberedRoadOrdinal Ordinal
);
