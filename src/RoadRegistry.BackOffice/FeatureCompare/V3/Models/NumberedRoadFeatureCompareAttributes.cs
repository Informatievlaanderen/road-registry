namespace RoadRegistry.BackOffice.FeatureCompare.V3.Models;

public record NumberedRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NumberedRoadNumber Number { get; init; }
    public RoadSegmentNumberedRoadDirection Direction { get; init; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; init; }
}
