namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record GetDownloadUploadPresignedUrlRequest(DownloadId DownloadId) : EndpointRequest<GetDownloadUploadPresignedUrlResponse>
{
}
