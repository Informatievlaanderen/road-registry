namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class ExtractDownloadNotFoundException : ApplicationException
{
    public ExtractDownloadNotFoundException(DownloadId downloadId) : base($"Could not find the download with identifier {downloadId}")
    {
    }
}
