namespace RoadRegistry.Extracts.FeatureCompare.V3.RoadSegmentSurface;

public record RoadSegmentSurfaceFeatureCompareAttributes : RoadSegmentDynamicAttributeAttributes
{
    public RoadSegmentSurfaceType Type { get; init; }
}
