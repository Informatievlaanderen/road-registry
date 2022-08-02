namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record UploadExtractResponse(UploadId UploadId) : EndpointResponse
{
}

public sealed record UploadExtractArchiveResponse(UploadId UploadId) : EndpointResponse
{
}
