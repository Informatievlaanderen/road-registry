namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public sealed record GetUploadFilePreSignedUrlRequest(string Identifier) : EndpointRequest<GetUploadFilePreSignedUrlResponse>
{
}
