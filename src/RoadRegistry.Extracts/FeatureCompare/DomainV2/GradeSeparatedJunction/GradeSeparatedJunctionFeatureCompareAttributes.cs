namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.GradeSeparatedJunction;

public record GradeSeparatedJunctionFeatureCompareAttributes
{
    public GradeSeparatedJunctionId Id { get; init; }
    public RoadSegmentTempId UpperRoadSegmentId { get; init; }
    public RoadSegmentTempId LowerRoadSegmentId { get; init; }
    public GradeSeparatedJunctionTypeV2 Type { get; init; }
}
