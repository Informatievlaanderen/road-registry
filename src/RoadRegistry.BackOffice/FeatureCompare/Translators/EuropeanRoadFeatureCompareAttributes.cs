namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public int EU_OIDN { get; init; }
    public string EUNUMMER { get; init; }
}
