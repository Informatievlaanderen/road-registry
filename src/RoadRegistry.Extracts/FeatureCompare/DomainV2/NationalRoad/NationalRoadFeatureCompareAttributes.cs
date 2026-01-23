namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.NationalRoad;

public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NationalRoadNumber Number { get; init; }
}
