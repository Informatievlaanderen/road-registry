namespace RoadRegistry.BackOffice.Abstractions.FeatureCompare;

using RoadRegistry.BackOffice.Abstractions;

public record FeatureCompareMessageResponse(ArchiveId ArchiveId) : SqsMessageResponse
{
}
