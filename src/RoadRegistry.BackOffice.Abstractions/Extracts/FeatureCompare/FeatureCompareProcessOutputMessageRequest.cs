namespace RoadRegistry.BackOffice.Abstractions.Extracts.FeatureCompare;

public record FeatureCompareProcessOutputMessageRequest(string ArchiveId,  Guid DownloadId, string RequestId, Guid UploadId) : SqsMessageRequest<FeatureCompareProcessOutputMessageResponse>;
