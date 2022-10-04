namespace RoadRegistry.BackOffice.Abstractions.FeatureCompare;

using RoadRegistry.BackOffice.Abstractions;

public record FeatureCompareMessageRequest(string ArchiveId) : SqsMessageRequest<FeatureCompareMessageResponse>
{
}
