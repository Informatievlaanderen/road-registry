namespace RoadRegistry.BackOffice.Abstractions.FeatureCompare;

public record FeatureCompareProcessOutputMessageResponse(ArchiveId ArchiveId) : SqsMessageResponse
{
}
