namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record ExtractListResponse : EndpointResponse
{
    public ICollection<ExtractListItem> Items { get; init; }
    public bool MoreDataAvailable { get; init; }
}

public sealed record ExtractListItem
{
    public DownloadId DownloadId { get; init; }
    public ExtractDescription Description { get; init; }
    public ExtractRequestId ExtractRequestId { get; init; }
    public DateTimeOffset RequestedOn { get; init; }
    public bool IsInformative { get; init; }
    public string DownloadStatus { get; init; }
    public string? UploadStatus { get; init; }
    public bool Closed { get; init; }
}
