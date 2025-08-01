namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record GetDownloadUploadPreSignedUrlResponse(string PreSignedUrl, string FileName) : EndpointResponse
{
}
