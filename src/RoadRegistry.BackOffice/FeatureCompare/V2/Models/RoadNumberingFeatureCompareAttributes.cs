namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

public abstract record RoadNumberingFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentId RoadSegmentId { get; init; }
}
