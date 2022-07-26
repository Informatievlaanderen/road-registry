namespace RoadRegistry.Api.BackOffice.Handlers.Exceptions;

public class DownloadExtractByContourNotFoundException : DownloadExtractNotFoundException
{
    public string Contour { get; init; }
}
