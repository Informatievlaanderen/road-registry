namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record GetDownloadExtractPresignedUrlResponse(string PresignedUrl, string FileName) : EndpointResponse
{
}
