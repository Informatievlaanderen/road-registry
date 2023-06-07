namespace RoadRegistry.BackOffice.Abstractions.Extracts.FeatureCompare;

public record FeatureCompareProcessOutputMessageResponse(ArchiveId ArchiveId) : SqsMessageResponse
{
}
