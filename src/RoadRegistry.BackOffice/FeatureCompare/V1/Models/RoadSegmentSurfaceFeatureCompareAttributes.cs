namespace RoadRegistry.BackOffice.FeatureCompare.V1.Models;

public record RoadSegmentSurfaceFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentSurfaceType Type { get; init; }
}
