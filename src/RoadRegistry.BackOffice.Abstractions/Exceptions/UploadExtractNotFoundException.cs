namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class UploadExtractNotFoundException : Exception
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
