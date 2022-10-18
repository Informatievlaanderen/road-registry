namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public sealed record DownloadExtractRequest(string Identifier) : EndpointRequest<DownloadExtractResponse>
{
}