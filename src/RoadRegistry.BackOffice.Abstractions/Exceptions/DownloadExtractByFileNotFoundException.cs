namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class DownloadExtractByFileNotFoundException : DownloadExtractNotFoundException
{
    public DownloadExtractByFileNotFoundException(string? message)
        : base(message ?? "Could not find download extract with the specified contour")
    {
    }
}
