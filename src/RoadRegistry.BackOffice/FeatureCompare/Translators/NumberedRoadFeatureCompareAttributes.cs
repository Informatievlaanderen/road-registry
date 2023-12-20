namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record NumberedRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public NumberedRoadNumber Number { get; init; }
    public RoadSegmentNumberedRoadDirection Direction { get; init; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; init; }
}
