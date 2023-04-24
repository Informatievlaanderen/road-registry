namespace RoadRegistry.BackOffice.Abstractions.Uploads.FeatureCompare;

public record FeatureCompareProcessOutputMessageRequest(string ArchiveId) : SqsMessageRequest<FeatureCompareProcessOutputMessageResponse>
{
}
