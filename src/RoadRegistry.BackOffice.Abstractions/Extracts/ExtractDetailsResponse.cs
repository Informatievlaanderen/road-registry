namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using NetTopologySuite.Geometries;

public sealed record ExtractDetailsResponse : EndpointResponse
{
    public DownloadId DownloadId { get; init; }
    public ExtractDescription Description { get; init; }
    public MultiPolygon Contour { get; init; }
    public ExtractRequestId ExtractRequestId { get; init; }
    public DateTimeOffset RequestedOn { get; set; }
    public bool IsInformative { get; init; }
}
