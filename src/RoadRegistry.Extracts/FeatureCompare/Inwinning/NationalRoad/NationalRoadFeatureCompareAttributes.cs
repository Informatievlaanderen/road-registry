namespace RoadRegistry.Extracts.FeatureCompare.Inwinning.NationalRoad;

public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NationalRoadNumber Number { get; init; }
}
