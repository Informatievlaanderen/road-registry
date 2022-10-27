namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public record UploadExtractResponse(UploadId UploadId) : EndpointResponse
{
}

public sealed record UploadExtractArchiveResponse(UploadId UploadId) : EndpointResponse
{
}