namespace RoadRegistry.BackOffice.FeatureCompare.V2.Models;

public record EuropeanRoadFeatureCompareAttributes: RoadNumberingFeatureCompareAttributes
{
    public EuropeanRoadNumber Number { get; init; }
}
