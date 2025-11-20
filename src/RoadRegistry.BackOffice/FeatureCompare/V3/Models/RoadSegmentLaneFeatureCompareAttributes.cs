namespace RoadRegistry.BackOffice.FeatureCompare.V3.Models;

public record RoadSegmentLaneFeatureCompareAttributes : RoadSegmentAttributeFeatureCompareAttributes
{
    public RoadSegmentLaneCount Count { get; init; }
    public RoadSegmentLaneDirection Direction { get; init; }
}
