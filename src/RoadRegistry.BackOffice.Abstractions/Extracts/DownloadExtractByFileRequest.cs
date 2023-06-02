namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractByFileRequest(DownloadExtractByFileRequestItem ShpFile, DownloadExtractByFileRequestItem PrjFile, int Buffer, string Description, bool IsInformative) : EndpointRequest<DownloadExtractByFileResponse>
{
}
