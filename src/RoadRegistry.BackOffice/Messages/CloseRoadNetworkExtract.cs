namespace RoadRegistry.BackOffice.Messages;

public class CloseRoadNetworkExtract
{
    public DownloadId DownloadId { get; set; }
    public RoadNetworkExtractCloseReason Reason { get; set; }
    public ExternalExtractRequestId ExternalRequestId { get; set; }
}
