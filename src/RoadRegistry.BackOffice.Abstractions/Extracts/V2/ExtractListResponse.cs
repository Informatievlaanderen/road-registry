namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record ExtractListResponse : EndpointResponse
{
    public ICollection<ExtractListItem> Items { get; set; }
}

public sealed record ExtractListItem
{
    public DownloadId DownloadId { get; init; }
    public ExtractDescription Description { get; init; }
    public ExtractRequestId ExtractRequestId { get; init; }
    public DateTimeOffset RequestedOn { get; set; }
    public bool IsInformative { get; init; }
    public string Status { get; init; }
    public bool Closed { get; init; }
}
