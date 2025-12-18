namespace RoadRegistry.Extracts.FeatureCompare.V3;

public abstract record RoadSegmentDynamicAttributeAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentId RoadSegmentId { get; init; }
    public RoadSegmentPosition FromPosition { get; init; }
    public RoadSegmentPosition ToPosition { get; init; }
}
