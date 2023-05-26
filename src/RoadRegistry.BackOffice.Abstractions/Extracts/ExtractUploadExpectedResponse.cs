namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record ExtractUploadExpectedResponse : EndpointResponse
{
    public DownloadId DownloadId { get; init; }
    public ExtractDescription Description { get; init; }
    public bool UploadExpected { get; init; }

}
