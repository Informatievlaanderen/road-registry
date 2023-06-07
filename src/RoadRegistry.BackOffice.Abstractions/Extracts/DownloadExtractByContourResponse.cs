namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractByContourResponse(DownloadId DownloadId, bool IsInformative) : EndpointResponse
{
}
