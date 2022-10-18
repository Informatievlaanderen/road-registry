namespace RoadRegistry.BackOffice.Abstractions.FeatureCompare;

public record FeatureCompareMessageResponse(ArchiveId ArchiveId) : SqsMessageResponse
{
}