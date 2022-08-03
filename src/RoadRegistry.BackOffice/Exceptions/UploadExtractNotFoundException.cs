namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class UploadExtractNotFoundException : ApplicationException
{
    public UploadExtractNotFoundException(string? message) : base(message)
    {
    }

    public UploadExtractNotFoundException(int retryAfterSeconds)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public int RetryAfterSeconds { get; init; }
}
