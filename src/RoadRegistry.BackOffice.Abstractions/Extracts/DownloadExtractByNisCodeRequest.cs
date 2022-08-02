namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractByNisCodeRequest(string NisCode, int Buffer, string Description) : EndpointRequest<DownloadExtractByNisCodeResponse>
{
}
