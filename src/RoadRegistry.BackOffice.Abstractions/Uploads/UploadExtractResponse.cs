namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public record UploadExtractResponse(ArchiveId ArchiveId) : EndpointResponse
{
}

public sealed record UploadExtractArchiveResponse(UploadId UploadId) : EndpointResponse
{
}