namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractByFileRequest(IReadOnlyCollection<DownloadExtractByFileRequestItem> Files, int Buffer, string Description) : EndpointRequest<DownloadExtractByFileResponse>
{
}
