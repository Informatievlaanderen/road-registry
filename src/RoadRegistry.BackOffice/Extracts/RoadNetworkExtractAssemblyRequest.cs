namespace RoadRegistry.BackOffice.Extracts;

using System;
using NetTopologySuite.Geometries;

public class RoadNetworkExtractAssemblyRequest
{
    public RoadNetworkExtractAssemblyRequest(ExternalExtractRequestId requestId, DownloadId downloadId, ExtractDescription extractDescription, IPolygonal contour)
    {
        ExternalRequestId = requestId;
        RequestId = ExtractRequestId.FromExternalRequestId(requestId);
        DownloadId = downloadId;
        ExtractDescription = extractDescription;
        Contour = contour ?? throw new ArgumentNullException(nameof(contour));
    }

    public IPolygonal Contour { get; }
    public DownloadId DownloadId { get; }
    public ExternalExtractRequestId ExternalRequestId { get; }
    public ExtractDescription ExtractDescription { get; }
    public ExtractRequestId RequestId { get; }
}