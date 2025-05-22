namespace RoadRegistry.BackOffice.Messages;

using System;

public class RequestRoadNetworkExtract
{
    public RoadNetworkExtractGeometry Contour { get; set; }
    public string Description { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public bool IsInformative { get; set; }
    public string ZipArchiveWriterVersion { get; set; }
}
