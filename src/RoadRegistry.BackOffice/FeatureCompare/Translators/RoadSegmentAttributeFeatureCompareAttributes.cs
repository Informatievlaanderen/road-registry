namespace RoadRegistry.BackOffice.FeatureCompare.Translators;
public abstract record RoadSegmentAttributeFeatureCompareAttributes
{
    public int Id { get; init; }
    public int WS_OIDN { get; set; }
    public double VANPOS { get; init; }
    public double TOTPOS { get; init; }
}
