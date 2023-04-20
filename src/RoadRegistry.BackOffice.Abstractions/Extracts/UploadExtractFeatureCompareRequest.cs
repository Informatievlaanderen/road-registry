using RoadRegistry.BackOffice.Abstractions.Uploads;

namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public record UploadExtractFeatureCompareRequest(string DownloadId, UploadExtractArchiveRequest Archive)
    : EndpointRequest<UploadExtractFeatureCompareResponse>, IUploadExtractFeatureCompareRequest;
