namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractByFileResponse(DownloadId DownloadId) : EndpointResponse
{
}