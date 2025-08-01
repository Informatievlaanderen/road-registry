namespace RoadRegistry.Extracts.Schema;

using System;
using NetTopologySuite.Geometries;

//TODO-pr add migration
public class ExtractRequest
{
    public string ExtractRequestId { get; set; }
    public string OrganizationCode { get; set; }
    public Geometry Contour { get; set; }
    public string Description { get; set; }
    //public string ExternalRequestId { get; set; }
    public bool IsInformative { get; set; }
    public DateTimeOffset RequestedOn { get; set; }
    public Guid DownloadId { get; set; }
    public bool DownloadAvailable { get; set; }
    public bool ExtractDownloadTimeoutOccurred { get; set; }
    public DateTimeOffset? DownloadedOn { get; set; }
    public string? ArchiveId { get; set; }
    public Guid? TicketId { get; set; }
    public bool Closed { get; set; }
}
