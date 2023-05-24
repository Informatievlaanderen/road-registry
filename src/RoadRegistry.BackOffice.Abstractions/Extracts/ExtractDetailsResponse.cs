namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using NetTopologySuite.Geometries;

public sealed record ExtractDetailsResponse : EndpointResponse
{
    public DownloadId DownloadId { get; init; }
    public ExtractDescription Description { get; init; }
    public Geometry Contour { get; init; }
    public ExtractRequestId ExtractRequestId { get; init; }
    public DateTime RequestOn { get; set; }
    public bool UploadExpected { get; init; }
}
