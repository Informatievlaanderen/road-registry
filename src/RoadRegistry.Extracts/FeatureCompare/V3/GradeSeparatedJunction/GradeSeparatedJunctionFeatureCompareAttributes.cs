namespace RoadRegistry.Extracts.FeatureCompare.V3.GradeSeparatedJunction;

public record GradeSeparatedJunctionFeatureCompareAttributes
{
    public GradeSeparatedJunctionId Id { get; init; }
    public RoadSegmentId UpperRoadSegmentId { get; init; }
    public RoadSegmentId LowerRoadSegmentId { get; init; }
    public GradeSeparatedJunctionType Type { get; init; }
}
