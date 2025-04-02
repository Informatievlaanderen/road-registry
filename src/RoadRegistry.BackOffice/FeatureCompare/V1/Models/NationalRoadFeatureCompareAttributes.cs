namespace RoadRegistry.BackOffice.FeatureCompare.V1.Translators;

public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NationalRoadNumber Number { get; init; }
}
