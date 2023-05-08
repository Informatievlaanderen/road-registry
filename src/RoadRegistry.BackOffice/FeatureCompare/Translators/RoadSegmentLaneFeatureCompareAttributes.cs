namespace RoadRegistry.BackOffice.FeatureCompare.Translators;
internal record RoadSegmentLaneFeatureCompareAttributes: RoadSegmentAttributeFeatureCompareAttributes
{
    public int AANTAL { get; init; }
    public int RICHTING { get; init; }
}
