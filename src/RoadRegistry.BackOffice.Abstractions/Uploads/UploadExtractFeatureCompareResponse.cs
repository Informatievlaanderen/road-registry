namespace RoadRegistry.BackOffice.Abstractions.Uploads;

using BackOffice.Uploads;

public record UploadExtractFeatureCompareResponse(ArchiveId ArchiveId, RoadNetworkChangesArchive Message) : EndpointResponse
{

}
