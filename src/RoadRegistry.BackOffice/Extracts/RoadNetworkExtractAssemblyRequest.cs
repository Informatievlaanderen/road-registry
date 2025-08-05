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
        bool isInformative,
        string zipArchiveWriterVersion)
    {
        ExternalRequestId = requestId;
        DownloadId = downloadId;
        ExtractDescription = extractDescription;
        Contour = contour ?? throw new ArgumentNullException(nameof(contour));
        IsInformative = isInformative;
        ZipArchiveWriterVersion = zipArchiveWriterVersion;
    }

    public ExternalExtractRequestId ExternalRequestId { get; }
    public DownloadId DownloadId { get; }
    public ExtractDescription ExtractDescription { get; }
    public IPolygonal Contour { get; }
    public bool IsInformative { get; }
    public string ZipArchiveWriterVersion { get; }
}
