namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record RoadSegmentWidthFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentWidth Width { get; init; }
}
