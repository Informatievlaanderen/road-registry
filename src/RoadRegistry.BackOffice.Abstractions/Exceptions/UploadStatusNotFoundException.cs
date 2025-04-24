namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class UploadStatusNotFoundException : Exception
{
    public UploadStatusNotFoundException(string? message)
        : base(message)
    {
    }
}
