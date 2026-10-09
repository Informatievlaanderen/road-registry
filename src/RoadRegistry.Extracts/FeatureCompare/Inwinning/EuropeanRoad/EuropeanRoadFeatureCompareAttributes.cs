namespace RoadRegistry.Extracts.FeatureCompare.Inwinning.EuropeanRoad;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public EuropeanRoadNumber Number { get; init; }
}
