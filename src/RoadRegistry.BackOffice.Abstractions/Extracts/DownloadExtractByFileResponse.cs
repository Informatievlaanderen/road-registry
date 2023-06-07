namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractByFileResponse(DownloadId DownloadId, bool IsInformative) : EndpointResponse
{
}
