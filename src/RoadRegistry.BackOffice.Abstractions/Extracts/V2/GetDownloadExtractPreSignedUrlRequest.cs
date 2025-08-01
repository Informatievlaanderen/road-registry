namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record GetDownloadExtractPreSignedUrlRequest(DownloadId DownloadId) : EndpointRequest<GetDownloadExtractPreSignedUrlResponse>
{
}
