namespace RoadRegistry.BackOffice.Contracts.Extracts;

public sealed record DownloadExtractByNisCodeRequest(string NisCode, int Buffer, string Description) : EndpointRequest<DownloadExtractByNisCodeResponse>
{
}
