namespace RoadRegistry.BackOffice.FeatureCompare.V1.Models;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public EuropeanRoadNumber Number { get; init; }
}
