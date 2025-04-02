namespace RoadRegistry.BackOffice.FeatureCompare.V1.Translators;

public record RoadSegmentWidthFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentWidth Width { get; init; }
}
