namespace RoadRegistry.Extracts.FeatureCompare.Inwinning.GradeSeparatedJunction;

public record GradeSeparatedJunctionFeatureCompareAttributes
{
    public GradeSeparatedJunctionId Id { get; init; }
    public RoadSegmentTempId UpperRoadSegmentTempId { get; init; }
    public RoadSegmentTempId LowerRoadSegmentTempId { get; init; }
    public GradeSeparatedJunctionTypeV2 Type { get; init; }
}
