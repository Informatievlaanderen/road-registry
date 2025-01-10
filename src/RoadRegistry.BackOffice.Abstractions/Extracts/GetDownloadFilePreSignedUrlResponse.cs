namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetDownloadFilePreSignedUrlResponse(string PresignedUrl) : EndpointResponse
{
}
