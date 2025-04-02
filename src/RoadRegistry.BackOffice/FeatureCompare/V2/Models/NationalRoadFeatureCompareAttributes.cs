namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NationalRoadNumber Number { get; init; }
}
