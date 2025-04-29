namespace RoadRegistry.BackOffice.Exceptions;

public class ExtractRequestNotFoundException : RoadRegistryException
{
    public DownloadId DownloadId { get; private set; }

    public ExtractRequestNotFoundException(DownloadId downloadId)
        : this(downloadId, $"Extract request with download ID {downloadId} could not be found.")
    {
    }

    public ExtractRequestNotFoundException(DownloadId downloadId, string message)
        : base(message)
    {
        DownloadId = downloadId;
    }

    public ExtractRequestNotFoundException(DownloadId downloadId, int retryAfterSeconds)
        : this(downloadId, "Extract request could not be found")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public int RetryAfterSeconds { get; init; }

}
