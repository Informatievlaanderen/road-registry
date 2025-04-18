namespace RoadRegistry.BackOffice.FeatureCompare.V2.Models;

public record RoadSegmentSurfaceFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentSurfaceType Type { get; init; }
}
