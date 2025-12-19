namespace RoadRegistry.BackOffice.Exceptions;

public sealed class InwinningszoneCompletedException : RoadRegistryException
{
    public InwinningszoneCompletedException(DownloadId downloadId)
        : base($"Inwinningszone with download ID {downloadId} is completed.")
    { }
}
