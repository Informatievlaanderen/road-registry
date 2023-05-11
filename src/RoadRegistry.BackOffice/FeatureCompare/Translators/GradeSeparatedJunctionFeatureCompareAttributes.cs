namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record GradeSeparatedJunctionFeatureCompareAttributes
{
    public int OK_OIDN { get; init; }
    public int BO_WS_OIDN { get; init; }
    public int ON_WS_OIDN { get; init; }
    public int TYPE { get; init; }
}
