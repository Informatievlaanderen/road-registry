namespace RoadRegistry.BackOffice.FeatureCompare.V3.Models;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public EuropeanRoadNumber Number { get; init; }
}
