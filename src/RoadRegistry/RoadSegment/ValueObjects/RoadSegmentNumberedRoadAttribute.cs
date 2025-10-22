namespace RoadRegistry.RoadSegment.ValueObjects;

using BackOffice;

public record struct RoadSegmentNumberedRoadAttribute(
    AttributeId AttributeId,
    RoadSegmentNumberedRoadDirection Direction,
    NumberedRoadNumber Number,
    RoadSegmentNumberedRoadOrdinal Ordinal
);
