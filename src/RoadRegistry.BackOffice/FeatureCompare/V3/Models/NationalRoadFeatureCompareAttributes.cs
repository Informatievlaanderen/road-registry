namespace RoadRegistry.BackOffice.FeatureCompare.V3.Models;

public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NationalRoadNumber Number { get; init; }
}
