namespace RoadRegistry.BackOffice.Messages
{
    using System;

    public class RequestRoadNetworkExtract
    {
        public string ExternalRequestId { get; set; }
        public Guid DownloadId { get; set; }
        public RoadNetworkExtractGeometry Contour { get; set; }
        public string Description { get; set; }
    }
}
