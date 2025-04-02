namespace RoadRegistry.BackOffice.FeatureCompare.V1.Translators;

public abstract record RoadNumberingFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentId RoadSegmentId { get; init; }
}
