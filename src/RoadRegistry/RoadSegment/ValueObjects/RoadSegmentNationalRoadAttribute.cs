namespace RoadRegistry.RoadSegment.ValueObjects;

using BackOffice;

public record struct RoadSegmentNationalRoadAttribute(
    AttributeId AttributeId,
    NationalRoadNumber Number
);
