namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractByFileResponse(DownloadId DownloadId, bool UploadExpected) : EndpointResponse
{
}
