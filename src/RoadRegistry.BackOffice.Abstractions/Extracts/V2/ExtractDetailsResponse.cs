namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

using NetTopologySuite.Geometries;

public sealed record ExtractDetailsResponse : EndpointResponse
{
    public DownloadId DownloadId { get; init; }
    public ExtractDescription Description { get; init; }
    public MultiPolygon Contour { get; init; }
    public ExtractRequestId ExtractRequestId { get; init; }
    public DateTimeOffset RequestedOn { get; init; }
    public bool IsInformative { get; init; }
    public string DownloadStatus { get; init; }
    public DateTimeOffset? DownloadedOn { get; init; }
    public string? UploadStatus { get; init; }
    public UploadId? UploadId { get; init; }
    public TicketId? TicketId { get; init; }
    public bool Closed { get; init; }
}
