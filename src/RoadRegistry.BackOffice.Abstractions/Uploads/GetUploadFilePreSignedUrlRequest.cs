namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public sealed record GetUploadFilePreSignedUrlRequest(DownloadId DownloadId) : EndpointRequest<GetUploadFilePreSignedUrlResponse>
{
}
