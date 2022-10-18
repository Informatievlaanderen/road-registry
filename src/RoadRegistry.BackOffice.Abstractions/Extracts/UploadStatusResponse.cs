namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record UploadStatusResponse(string Status, int RetryAfter) : EndpointResponse
{
}