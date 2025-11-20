namespace RoadRegistry.BackOffice.FeatureCompare.V3.EuropeanRoad;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public EuropeanRoadNumber Number { get; init; }
}
