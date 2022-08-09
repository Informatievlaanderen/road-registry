namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class UploadStatusNotFoundException : ApplicationException
{
    public UploadStatusNotFoundException(string? message) : base(message)
    {
    }
}
