namespace RoadRegistry.BackOffice.FeatureCompare.V1.Translators;

public record RoadSegmentSurfaceFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentSurfaceType Type { get; init; }
}
