namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record GetDownloadExtractPresignedUrlRequest(DownloadId DownloadId) : EndpointRequest<GetDownloadExtractPresignedUrlResponse>
{
}
