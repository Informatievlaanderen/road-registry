namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public sealed record GetUploadFilePreSignedUrlResponse(string PreSignedUrl, string FileName) : EndpointResponse
{
}
