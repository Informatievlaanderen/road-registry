namespace RoadRegistry.BackOffice.Abstractions.Uploads;

using Be.Vlaanderen.Basisregisters.BlobStore;

public record UploadExtractRequest(UploadExtractArchiveRequest Archive) : EndpointRequest<UploadExtractResponse>
{
    public bool UseZipArchiveFeatureCompareTranslator { get; set; }
}

public record UploadExtractArchiveRequest(string FileName, Stream ReadStream, ContentType ContentType) : EndpointRequest<UploadExtractArchiveResponse>;
