namespace RoadRegistry.BackOffice.Abstractions.FeatureCompare;

public record FeatureCompareProcessOutputMessageRequest(string ArchiveId) : SqsMessageRequest<FeatureCompareProcessOutputMessageResponse>
{
}
