namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Net;

public sealed class DownloadEditorNotFoundException : Exception
{
    public DownloadEditorNotFoundException(string? message, HttpStatusCode statusCode)
        : base(message)
    {
        HttpStatusCode = statusCode;
    }

    public HttpStatusCode HttpStatusCode { get; init; }
}
