namespace RoadRegistry.Integration.Schema.RoadNetworkChanges;

public class RoadNetworkExtractDownloadTimeoutOccurredEntry
{
    public string RequestId { get; set; }
    public string ExternalRequestId { get; set; }
    public string Description { get; set; }
    public bool IsInformative { get; set; }
}
