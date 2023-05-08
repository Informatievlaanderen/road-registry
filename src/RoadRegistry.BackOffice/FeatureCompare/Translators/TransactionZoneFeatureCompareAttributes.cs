namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record TransactionZoneFeatureCompareAttributes
{
    public string BESCHRIJV { get; init; }
    public string DOWNLOADID { get; init; }
    public string OPERATOR { get; init; }
    public string ORG { get; init; }
    public int TYPE { get; init; }
}
