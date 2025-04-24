namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class DownloadExtractNotFoundException : DownloadExtractException
{
    public DownloadExtractNotFoundException()
        : this("Download extract could not be found")
    {
    }

    public DownloadExtractNotFoundException(string? message)
        : base(message ?? "Download extract could not be found")
    {
    }

    public DownloadExtractNotFoundException(int retryAfterSeconds)
        : this("Download extract could not be found")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public int RetryAfterSeconds { get; init; }
}
