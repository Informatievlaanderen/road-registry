namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public class DownloadExtractNotFoundException : DownloadExtractException
{
    public DownloadExtractNotFoundException(string? message)
        : base(message)
    { }

    public DownloadExtractNotFoundException(int retryAfterSeconds)
        : this("Download extract could not be found")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    protected DownloadExtractNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public int RetryAfterSeconds { get; init; }
}
