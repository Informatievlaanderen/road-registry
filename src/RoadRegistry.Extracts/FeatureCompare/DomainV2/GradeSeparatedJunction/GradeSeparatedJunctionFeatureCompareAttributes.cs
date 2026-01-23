namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.GradeSeparatedJunction;

public record GradeSeparatedJunctionFeatureCompareAttributes
{
    public GradeSeparatedJunctionId Id { get; init; }
    public RoadSegmentId UpperRoadSegmentId { get; init; }
    public RoadSegmentId LowerRoadSegmentId { get; init; }
    public GradeSeparatedJunctionTypeV2 Type { get; init; }
}
