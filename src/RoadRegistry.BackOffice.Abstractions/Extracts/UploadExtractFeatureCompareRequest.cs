namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using Uploads;

public record UploadExtractFeatureCompareRequest(string DownloadId, UploadExtractArchiveRequest Archive)
    : EndpointRequest<UploadExtractFeatureCompareResponse>;
