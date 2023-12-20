namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public EuropeanRoadNumber Number { get; init; }
}
