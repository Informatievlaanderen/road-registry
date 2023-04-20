namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public sealed record UploadExtractFeatureCompareRequest(string DownloadId, UploadExtractArchiveRequest Archive)
    : EndpointRequest<UploadExtractFeatureCompareResponse>, IUploadExtractFeatureCompareRequest;
