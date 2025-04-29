namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class UploadExtractException : Exception
{
    public UploadExtractException()
    {
    }

    public UploadExtractException(string? message) : base(message)
    {
    }
}
