namespace RoadRegistry.Extracts.FeatureCompare.V3.RoadSegmentSurface;

public record RoadSegmentSurfaceFeatureCompareAttributes : RoadSegmentDynamicAttributeAttributes
{
    public RoadSegmentSurfaceTypeV2 Type { get; init; }
}
