namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

using NetTopologySuite.Geometries;

public sealed record ExtractDetailsResponse : EndpointResponse
{
    public DownloadId DownloadId { get; init; }
    public ExtractDescription Description { get; init; }
    public MultiPolygon Contour { get; init; }
    public ExtractRequestId ExtractRequestId { get; init; }
    public DateTimeOffset RequestedOn { get; set; }
    public bool IsInformative { get; init; }
    public ArchiveId? ArchiveId { get; init; }
    public TicketId? TicketId { get; init; }
    public bool DownloadAvailable { get; init; }
    public bool ExtractDownloadTimeoutOccurred { get; init; }
}
