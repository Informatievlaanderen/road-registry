namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record RoadSegmentSurfaceFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentSurfaceType Type { get; init; }
}
