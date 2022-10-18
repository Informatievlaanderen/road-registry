namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Net;
using System.Runtime.Serialization;

[Serializable]
public sealed class DownloadEditorNotFoundException : ApplicationException
{
    public DownloadEditorNotFoundException(string? message, HttpStatusCode statusCode) : base(message)
    {
        HttpStatusCode = statusCode;
    }

    private DownloadEditorNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public HttpStatusCode HttpStatusCode { get; init; }
}
