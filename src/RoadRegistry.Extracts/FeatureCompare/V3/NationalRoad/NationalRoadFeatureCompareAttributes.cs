namespace RoadRegistry.Extracts.FeatureCompare.V3.NationalRoad;

public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NationalRoadNumber Number { get; init; }
}
