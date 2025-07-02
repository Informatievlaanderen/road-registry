namespace RoadRegistry.Editor.Schema.Extracts;

using System;
using NetTopologySuite.Geometries;

public class ExtractRequestRecord
{
    public Guid DownloadId { get; set; }
    public string Description { get; set; }
    public Geometry Contour { get; set; }
    public string ExternalRequestId { get; set; }
    public DateTimeOffset RequestedOn { get; set; }
    public bool IsInformative { get; set; }
    public DateTimeOffset? DownloadedOn { get; set; }
    public string ArchiveId { get; set; }
    public Guid? TicketId { get; set; }
    public bool DownloadAvailable { get; set; }
    public bool ExtractDownloadTimeoutOccurred { get; set; }
}
