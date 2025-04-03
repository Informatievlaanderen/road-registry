namespace RoadRegistry.BackOffice.FeatureCompare.V1.Models;

public record RoadSegmentWidthFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentWidth Width { get; init; }
}
