namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record GradeSeparatedJunctionFeatureCompareAttributes
{
    public int Id { get; init; }
    public int UpperRoadSegmentId { get; init; }
    public int LowerRoadSegmentId { get; init; }
    public int Type { get; init; }
}
