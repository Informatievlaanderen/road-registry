namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record TransactionZoneFeatureCompareAttributes
{
    public string Description { get; init; }
    public string DownloadId { get; init; }
    public string OperatorName { get; init; }
    public string Organization { get; init; }
    public int Type { get; init; }
}
