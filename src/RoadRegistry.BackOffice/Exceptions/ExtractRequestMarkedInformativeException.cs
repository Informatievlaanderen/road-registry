namespace RoadRegistry.BackOffice.Exceptions;

using System;

public sealed class ExtractRequestMarkedInformativeException : RoadRegistryException
{
    public ExtractRequestMarkedInformativeException(DownloadId downloadId)
        : this(downloadId.ToString())
    { }

    public ExtractRequestMarkedInformativeException(string identifier)
        : base($"Could not upload an informative extract request with identifier {identifier}")
    { }
}
