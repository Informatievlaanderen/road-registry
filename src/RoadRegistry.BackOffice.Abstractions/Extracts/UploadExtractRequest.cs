namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using Be.Vlaanderen.Basisregisters.BlobStore;

public record UploadExtractRequest(string DownloadId, UploadExtractArchiveRequest Archive, bool FeatureCompare) : EndpointRequest<UploadExtractResponse>
{
}

public record UploadExtractArchiveRequest(string FileName, Stream ReadStream, ContentType ContentType) : EndpointRequest<UploadExtractArchiveResponse>
{
}
