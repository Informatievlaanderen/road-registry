namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using Be.Vlaanderen.Basisregisters.BlobStore;

public record UploadExtractRequest(string DownloadId, UploadExtractArchiveRequest Archive) : EndpointRequest<UploadExtractResponse>
{
    public bool FeatureCompare { get; set; }
}

public record UploadExtractArchiveRequest(string FileName, Stream ReadStream, ContentType ContentType) : EndpointRequest<UploadExtractArchiveResponse>
{
}
