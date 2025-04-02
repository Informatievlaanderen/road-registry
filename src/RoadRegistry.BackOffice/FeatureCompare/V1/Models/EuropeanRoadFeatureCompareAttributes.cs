namespace RoadRegistry.BackOffice.FeatureCompare.V1.Translators;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public EuropeanRoadNumber Number { get; init; }
}
