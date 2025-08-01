namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record GetDownloadUploadPreSignedUrlRequest(DownloadId DownloadId) : EndpointRequest<GetDownloadUploadPreSignedUrlResponse>
{
}
