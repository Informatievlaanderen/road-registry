namespace RoadRegistry.Extracts.FeatureCompare.Inwinning.TransactionZone;

public record TransactionZoneFeatureCompareAttributes
{
    public ExtractGeometry Geometry { get; init; }
    public ExtractDescription Description { get; init; }
    public DownloadId DownloadId { get; init; }
    public OperatorName OperatorName { get; init; }
    public OrganizationId Organization { get; init; }
}
