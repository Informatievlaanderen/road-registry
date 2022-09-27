namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public record UploadExtractFeatureCompareRequest(string DownloadId, UploadExtractArchiveRequest Archive) : UploadExtractRequest(DownloadId, Archive)
{
}
