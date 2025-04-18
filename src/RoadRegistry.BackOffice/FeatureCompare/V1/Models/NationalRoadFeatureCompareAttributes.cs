namespace RoadRegistry.BackOffice.FeatureCompare.V1.Models;

public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NationalRoadNumber Number { get; init; }
}
