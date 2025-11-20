namespace RoadRegistry.BackOffice.FeatureCompare.V3.Models;

using RoadSegment.ValueObjects;

public abstract record RoadNumberingFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentId RoadSegmentId { get; init; }
}
