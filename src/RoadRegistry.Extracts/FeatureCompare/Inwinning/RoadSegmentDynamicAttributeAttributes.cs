namespace RoadRegistry.Extracts.FeatureCompare.Inwinning;

public abstract record RoadSegmentDynamicAttributeAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentId RoadSegmentId { get; init; }
    public RoadSegmentPositionV2 FromPosition { get; init; }
    public RoadSegmentPositionV2 ToPosition { get; init; }
}
