namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.EuropeanRoad;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public EuropeanRoadNumber Number { get; init; }
}
