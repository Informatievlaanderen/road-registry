namespace RoadRegistry.BackOffice.Messages;

public class CloseRoadNetworkExtract
{
    public ExternalExtractRequestId ExternalRequestId { get; set; }
    public RoadNetworkExtractCloseReason Reason { get; set; }
    public DownloadId? DownloadId { get; set; }
}
