namespace RoadRegistry.BackOffice.FeatureCompare.V2.Models;

public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NationalRoadNumber Number { get; init; }
}
