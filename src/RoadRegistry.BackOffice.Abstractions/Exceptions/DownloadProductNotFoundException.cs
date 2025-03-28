namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class DownloadProductNotFoundException : ApplicationException
{
    public DownloadProductNotFoundException()
        : base(string.Empty)
    {
    }

    private DownloadProductNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
