namespace RoadRegistry.BackOffice.FeatureCompare.V1.Models;

public record GradeSeparatedJunctionFeatureCompareAttributes
{
    public GradeSeparatedJunctionId Id { get; init; }
    public RoadSegmentId UpperRoadSegmentId { get; init; }
    public RoadSegmentId LowerRoadSegmentId { get; init; }
    public GradeSeparatedJunctionType Type { get; init; }
}
