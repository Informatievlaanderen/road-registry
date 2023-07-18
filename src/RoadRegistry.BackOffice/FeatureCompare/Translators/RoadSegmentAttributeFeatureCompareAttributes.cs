namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public abstract class RoadSegmentAttributeFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public RoadSegmentId RoadSegmentId { get; set; }
    public RoadSegmentPosition FromPosition { get; init; }
    public RoadSegmentPosition ToPosition { get; init; }
}
