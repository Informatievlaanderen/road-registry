namespace RoadRegistry.BackOffice.FeatureCompare.V3.Models;

public record RoadSegmentSurfaceFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentSurfaceType Type { get; init; }
}
