namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

public abstract record RoadSegmentAttributeFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentId RoadSegmentId { get; init; }
    public RoadSegmentPosition FromPosition { get; init; }
    public RoadSegmentPosition ToPosition { get; init; }
}
