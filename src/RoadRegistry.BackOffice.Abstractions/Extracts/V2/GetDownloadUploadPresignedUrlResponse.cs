namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record GetDownloadUploadPresignedUrlResponse(string PresignedUrl, string FileName) : EndpointResponse
{
}
