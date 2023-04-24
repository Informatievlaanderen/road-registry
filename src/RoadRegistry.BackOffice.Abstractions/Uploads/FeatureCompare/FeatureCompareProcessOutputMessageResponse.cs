namespace RoadRegistry.BackOffice.Abstractions.Uploads.FeatureCompare;

public record FeatureCompareProcessOutputMessageResponse(ArchiveId ArchiveId) : SqsMessageResponse
{
}
