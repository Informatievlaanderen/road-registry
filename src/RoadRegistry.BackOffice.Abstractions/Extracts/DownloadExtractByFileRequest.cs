namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractByFileRequest(DownloadExtractByFileRequestItem ShpFile, DownloadExtractByFileRequestItem ShxFile, DownloadExtractByFileRequestItem PrjFile, int Buffer, string Description) : EndpointRequest<DownloadExtractByFileResponse>
{
}
