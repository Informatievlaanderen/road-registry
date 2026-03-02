namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

public abstract record RoadNumberingFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentTempId RoadSegmentTempId { get; init; }
}
