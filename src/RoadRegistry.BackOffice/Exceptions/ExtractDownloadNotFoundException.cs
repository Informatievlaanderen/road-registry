namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class ExtractDownloadNotFoundException : ApplicationException
{
    public ExtractDownloadNotFoundException(DownloadId downloadId) : this(downloadId.ToString())
    {
    }

    public ExtractDownloadNotFoundException(string identifier) : base($"Could not find the download with identifier {identifier}")
    {
    }
}
