namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NationalRoadNumber Number { get; init; }
}
