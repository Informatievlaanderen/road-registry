namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class ExtractDownloadNotFoundException : ApplicationException
{
    public ExtractDownloadNotFoundException(DownloadId downloadId)
        : this(downloadId.ToString())
    { }

    private ExtractDownloadNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public ExtractDownloadNotFoundException(string identifier)
        : base($"Could not find the download with identifier {identifier}")
    { }
}
