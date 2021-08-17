namespace RoadRegistry.BackOffice.Extracts
{
    using System;
    using NetTopologySuite.Geometries;

    public class RoadNetworkExtractAssemblyRequest
    {
        public ExternalExtractRequestId ExternalRequestId { get; }
        public ExtractRequestId RequestId { get; }
        public DownloadId DownloadId { get; }
        public IPolygonal Contour { get; }

        public RoadNetworkExtractAssemblyRequest(ExternalExtractRequestId requestId, DownloadId downloadId, IPolygonal contour)
        {
            ExternalRequestId = requestId;
            RequestId = ExtractRequestId.FromExternalRequestId(requestId);
            DownloadId = downloadId;
            Contour = contour ?? throw new ArgumentNullException(nameof(contour));
        }
    }
}
