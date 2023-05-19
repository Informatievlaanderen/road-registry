namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record ExtractDetailsRequest(DownloadId DownloadId) : EndpointRequest<ExtractDetailsResponse>
{
}
