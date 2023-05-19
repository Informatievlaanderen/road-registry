namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractResponse(DownloadId DownloadId, bool UploadExpected) : EndpointResponse
{
}
