namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record GetDownloadExtractPreSignedUrlResponse(string PreSignedUrl) : EndpointResponse
{
}
