namespace RoadRegistry.BackOffice.Abstractions.FeatureCompare;

public record FeatureCompareMessageRequest(string ArchiveId) : SqsMessageRequest<FeatureCompareMessageResponse>
{
}