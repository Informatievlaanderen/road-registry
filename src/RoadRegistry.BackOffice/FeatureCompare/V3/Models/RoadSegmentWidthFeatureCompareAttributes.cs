namespace RoadRegistry.BackOffice.FeatureCompare.V3.Models;

public record RoadSegmentWidthFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentWidth Width { get; init; }
}
