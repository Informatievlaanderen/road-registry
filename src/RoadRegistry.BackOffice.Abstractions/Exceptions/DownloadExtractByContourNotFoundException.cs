namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class DownloadExtractByContourNotFoundException : DownloadExtractNotFoundException
{
    public DownloadExtractByContourNotFoundException(string? message)
        : base(message ?? "Could not find download extract with the specified contour")
    {
    }

    public string Contour { get; init; }
}
