namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class ExtractRequestMarkedInformativeException : ApplicationException
{
    public ExtractRequestMarkedInformativeException(DownloadId downloadId)
        : this(downloadId.ToString())
    { }

    private ExtractRequestMarkedInformativeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public ExtractRequestMarkedInformativeException(string identifier)
        : base($"Could not upload an informative extract request with identifier {identifier}")
    { }
}
