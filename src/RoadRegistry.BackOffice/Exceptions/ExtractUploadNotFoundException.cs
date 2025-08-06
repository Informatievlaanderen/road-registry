namespace RoadRegistry.BackOffice.Exceptions;

public sealed class ExtractUploadNotFoundException : RoadRegistryException
{
    public ExtractUploadNotFoundException(DownloadId downloadId)
        : this(downloadId.ToString())
    { }

    public ExtractUploadNotFoundException(string identifier)
        : base($"Could not find the upload for extract with identifier {identifier}")
    { }
}
