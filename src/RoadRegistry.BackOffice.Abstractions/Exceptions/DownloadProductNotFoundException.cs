namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class DownloadProductNotFoundException : ApplicationException
{
    public DownloadProductNotFoundException(string? message)
        : base(message)
    {
    }

    private DownloadProductNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}