namespace RoadRegistry.BackOffice.FeatureCompare.V1.Models;

public record NumberedRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public NumberedRoadNumber Number { get; init; }
    public RoadSegmentNumberedRoadDirection Direction { get; init; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; init; }
}
