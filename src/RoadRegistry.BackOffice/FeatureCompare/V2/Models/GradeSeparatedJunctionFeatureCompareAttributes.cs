namespace RoadRegistry.BackOffice.FeatureCompare.V2.Models;

using RoadSegment.ValueObjects;

public record GradeSeparatedJunctionFeatureCompareAttributes
{
    public GradeSeparatedJunctionId Id { get; init; }
    public RoadSegmentId UpperRoadSegmentId { get; init; }
    public RoadSegmentId LowerRoadSegmentId { get; init; }
    public GradeSeparatedJunctionType Type { get; init; }
}
