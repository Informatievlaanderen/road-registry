namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

public class ExtractDownloadNotFoundException : ApplicationException
{
    public ExtractDownloadNotFoundException(DownloadId downloadId)
        : this(downloadId.ToString())
    {
    }

    public ExtractDownloadNotFoundException(string identifier)
        : base($"Could not find the download with identifier {identifier}")
    {
    }

    private ExtractDownloadNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
