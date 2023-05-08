namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record NumberedRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public int GW_OIDN { get; init; }
    public string IDENT8 { get; init; }
    public int RICHTING { get; init; }
    public int VOLGNUMMER { get; init; }
}
