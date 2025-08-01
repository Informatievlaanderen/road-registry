namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record ExtractDetailsRequest(DownloadId DownloadId) : EndpointRequest<ExtractDetailsResponse>
{
}
