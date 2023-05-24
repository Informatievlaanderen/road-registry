namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record ExtractDetailsResponse : EndpointResponse
{
    public DownloadId DownloadId { get; init; }
    public ExtractRequestId ExtractRequestId { get; init; }
    public ExtractDescription Description { get; init; }
    public string ExternalRequestId { get; set; }
    public string RequestId { get; set; }
    public DateTime RequestOn { get; set; }
    public bool UploadExpected { get; init; }
}
