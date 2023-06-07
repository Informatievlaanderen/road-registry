namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using Uploads;

public record UploadExtractRequest(string DownloadId, UploadExtractArchiveRequest Archive) : EndpointRequest<UploadExtractResponse>
{
    public bool UseZipArchiveFeatureCompareTranslator { get; set; }
}
