namespace RoadRegistry.BackOffice.FeatureCompare.V2.Models;

public record RoadSegmentWidthFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentWidth Width { get; init; }
}
