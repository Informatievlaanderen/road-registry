namespace RoadRegistry.BackOffice.Contracts.Uploads;

public sealed record UploadExtractResponse(UploadId UploadId) : EndpointResponse
{
}

public sealed record UploadExtractArchiveResponse(UploadId UploadId) : EndpointResponse
{
}
