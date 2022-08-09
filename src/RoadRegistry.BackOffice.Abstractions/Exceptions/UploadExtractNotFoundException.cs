namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

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
