namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public EuropeanRoadNumber Number { get; init; }
}
