namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public class NumberedRoadFeatureCompareAttributes : RoadNumberingFeatureCompareAttributes
{
    public int Id { get; init; }
    public string Number { get; init; }
    public int Direction { get; init; }
    public int Ordinal { get; init; }
}
