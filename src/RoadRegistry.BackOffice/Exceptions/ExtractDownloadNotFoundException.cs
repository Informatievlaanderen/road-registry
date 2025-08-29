namespace RoadRegistry.BackOffice.Exceptions;

public sealed class ExtractDownloadNotFoundException : RoadRegistryException
{
    public ExtractDownloadNotFoundException(DownloadId downloadId)
        : this(downloadId.ToString())
    { }

    public ExtractDownloadNotFoundException(string identifier)
        : base($"Could not find the download with identifier {identifier}")
    { }

    public ExtractDownloadNotFoundException(int retryAfterSeconds)
        : this("Extract download could not be found")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public int RetryAfterSeconds { get; init; }
}
