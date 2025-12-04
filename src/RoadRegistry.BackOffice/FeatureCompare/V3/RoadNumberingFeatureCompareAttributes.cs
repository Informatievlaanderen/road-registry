namespace RoadRegistry.BackOffice.FeatureCompare.V3;

using RoadRegistry.RoadSegment.ValueObjects;

public abstract record RoadNumberingFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentId RoadSegmentId { get; init; }
}
