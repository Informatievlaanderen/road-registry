namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

public record TransactionZoneFeatureCompareAttributes
{
    public ExtractDescription Description { get; init; }
    public DownloadId DownloadId { get; init; }
    public OperatorName OperatorName { get; init; }
    public OrganizationId Organization { get; init; }
}
