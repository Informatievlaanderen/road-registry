namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class UploadExtractNotFoundException : ApplicationException
{
    public UploadExtractNotFoundException(string? message) : base(message)
    {
    }

    public UploadExtractNotFoundException(int retryAfterSeconds)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    private UploadExtractNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public int RetryAfterSeconds { get; init; }
}