namespace RoadRegistry.BackOffice.FeatureCompare.V2.Models;

public record TransactionZoneFeatureCompareAttributes
{
    public ExtractDescription Description { get; init; }
    public DownloadId DownloadId { get; init; }
    public OperatorName OperatorName { get; init; }
    public OrganizationId Organization { get; init; }
}
