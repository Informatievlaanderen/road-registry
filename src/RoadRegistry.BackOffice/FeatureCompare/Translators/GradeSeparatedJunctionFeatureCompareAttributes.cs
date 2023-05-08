namespace RoadRegistry.BackOffice.FeatureCompare.Translators;
public record GradeSeparatedJunctionFeatureCompareAttributes
{
    public int BO_WS_OIDN { get; set; }
    public int OK_OIDN { get; init; }
    public int ON_WS_OIDN { get; set; }
    public int TYPE { get; init; }
}
