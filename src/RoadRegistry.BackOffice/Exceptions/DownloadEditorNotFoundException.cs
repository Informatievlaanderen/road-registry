namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Net;

public class DownloadEditorNotFoundException : ApplicationException
{
    public DownloadEditorNotFoundException(string? message, HttpStatusCode statusCode) : base(message)
    {
        HttpStatusCode = statusCode;
    }

    public HttpStatusCode HttpStatusCode { get; init; }
}
