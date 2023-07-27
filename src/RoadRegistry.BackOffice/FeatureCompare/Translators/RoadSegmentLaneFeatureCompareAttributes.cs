namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record RoadSegmentLaneFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentLaneCount Count { get; init; }
    public RoadSegmentLaneDirection Direction { get; init; }
}
