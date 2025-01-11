namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetDownloadFilePreSignedUrlResponse(string PreSignedUrl) : EndpointResponse
{
}
