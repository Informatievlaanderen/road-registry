namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record CloseExtractRequest(Guid DownloadId) : EndpointRequest<CloseExtractResponse>
{
}
