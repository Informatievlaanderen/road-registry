namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

public record RoadSegmentWidthFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentWidth Width { get; init; }
}
