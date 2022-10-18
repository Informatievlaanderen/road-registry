namespace RoadRegistry.BackOffice.Abstractions.Uploads;

using Be.Vlaanderen.Basisregisters.BlobStore;

public record UploadExtractRequest(string DownloadId, UploadExtractArchiveRequest Archive) : EndpointRequest<UploadExtractResponse>
{
}

public record UploadExtractArchiveRequest(string FileName, Stream ReadStream, ContentType ContentType) : EndpointRequest<UploadExtractArchiveResponse>
{
}