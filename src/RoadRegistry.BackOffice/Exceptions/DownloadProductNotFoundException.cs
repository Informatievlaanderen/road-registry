namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class DownloadProductNotFoundException : ApplicationException
{
    public DownloadProductNotFoundException(string? message) : base(message)
    {
    }
}
