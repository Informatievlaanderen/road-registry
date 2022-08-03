namespace RoadRegistry.BackOffice.Exceptions;

public class DownloadExtractNotFoundException : DownloadExtractException
{
    public DownloadExtractNotFoundException(string? message) : base(message)
    {
    }

    public DownloadExtractNotFoundException(int retryAfterSeconds) : this("Download extract could not be found")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public int RetryAfterSeconds { get; init; }
}
