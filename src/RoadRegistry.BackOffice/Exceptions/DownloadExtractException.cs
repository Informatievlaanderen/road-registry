namespace RoadRegistry.BackOffice.Exceptions;

using System;

public abstract class DownloadExtractException : ApplicationException
{
    protected DownloadExtractException(string? message) : base(message)
    {
    }

    public string Description { get; init; }
}
