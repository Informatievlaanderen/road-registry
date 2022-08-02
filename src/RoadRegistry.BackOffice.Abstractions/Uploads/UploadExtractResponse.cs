namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public sealed record UploadExtractResponse(ArchiveId ArchiveId) : EndpointResponse
{
}

public sealed record UploadExtractArchiveResponse(UploadId UploadId) : EndpointResponse
{
}
