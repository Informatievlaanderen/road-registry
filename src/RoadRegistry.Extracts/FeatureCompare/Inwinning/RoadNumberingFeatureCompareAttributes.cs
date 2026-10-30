namespace RoadRegistry.Extracts.FeatureCompare.Inwinning;

public abstract record RoadNumberingFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentTempId RoadSegmentTempId { get; init; }
}
