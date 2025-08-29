namespace RoadRegistry.BackOffice.Exceptions;

public sealed class ExtractRequestClosedException : RoadRegistryException
{
    public ExtractRequestClosedException(DownloadId downloadId)
        : base($"Extract request with download ID {downloadId} is closed.")
    { }
}
