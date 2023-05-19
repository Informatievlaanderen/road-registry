namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record ExtractDetailsResponse : EndpointResponse
{
    public DownloadId DownloadId { get; init; }
    public ExtractRequestId ExtractRequestId { get; init; }
    public string Description { get; init; }
    public bool UploadExpected { get; init; }
}
