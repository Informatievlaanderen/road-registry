namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record NationalRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public AttributeId Id { get; init; }
    public NationalRoadNumber Number { get; init; }
}
