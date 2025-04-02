namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

public record RoadSegmentLaneFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentLaneCount Count { get; init; }
    public RoadSegmentLaneDirection Direction { get; init; }
}
