namespace RoadRegistry.Api.BackOffice.Handlers.Exceptions;

public class DownloadExtractByNisCodeNotFoundException : DownloadExtractNotFoundException
{
    public string NisCode { get; init; }
}
