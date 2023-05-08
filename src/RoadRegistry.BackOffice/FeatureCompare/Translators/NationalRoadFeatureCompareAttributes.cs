namespace RoadRegistry.BackOffice.FeatureCompare.Translators;
public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public string IDENT2 { get; init; }
    public int NW_OIDN { get; init; }
}
