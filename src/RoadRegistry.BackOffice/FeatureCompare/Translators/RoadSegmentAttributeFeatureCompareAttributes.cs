namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public abstract class RoadSegmentAttributeFeatureCompareAttributes
{
    public int? Id { get; init; }
    public int? RoadSegmentId { get; set; }
    public double? FromPosition { get; set; }
    public double? ToPosition { get; set; }
}
