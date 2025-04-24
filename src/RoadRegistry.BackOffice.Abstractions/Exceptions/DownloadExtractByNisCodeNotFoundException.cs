namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class DownloadExtractByNisCodeNotFoundException : DownloadExtractNotFoundException
{
    public DownloadExtractByNisCodeNotFoundException(string? message)
        : base(message ?? "Could not find download extract with the specified NIS code")
    { }

    public string NisCode { get; set; }
}
