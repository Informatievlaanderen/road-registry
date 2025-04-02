namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public EuropeanRoadNumber Number { get; init; }
}
