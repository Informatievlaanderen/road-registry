namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public abstract record RoadNumberingFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentId RoadSegmentId { get; init; }
}
