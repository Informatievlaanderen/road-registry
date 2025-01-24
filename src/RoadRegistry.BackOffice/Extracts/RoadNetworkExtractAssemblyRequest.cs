namespace RoadRegistry.BackOffice.Extracts;

using System;
using NetTopologySuite.Geometries;

public class RoadNetworkExtractAssemblyRequest
{
    public RoadNetworkExtractAssemblyRequest(
        ExternalExtractRequestId requestId,
        DownloadId downloadId,
        ExtractDescription extractDescription,
        IPolygonal contour,
        bool isInformative)
    {
        ExternalRequestId = requestId;
        RequestId = ExtractRequestId.FromExternalRequestId(requestId);
        DownloadId = downloadId;
        ExtractDescription = extractDescription;
        Contour = contour ?? throw new ArgumentNullException(nameof(contour));
        IsInformative = isInformative;
    }

    public ExternalExtractRequestId ExternalRequestId { get; }
    public ExtractRequestId RequestId { get; }
    public DownloadId DownloadId { get; }
    public ExtractDescription ExtractDescription { get; }
    public IPolygonal Contour { get; }
    public bool IsInformative { get; }
}
