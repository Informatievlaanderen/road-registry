namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class UploadExtractException : ApplicationException
{
    public UploadExtractException()
    {
    }

    public UploadExtractException(string? message) : base(message)
    {
    }
}

public class UploadExtractNullException : UploadExtractException
{
    public UploadExtractNullException()
    {
    }

    public UploadExtractNullException(string? message) : base(message)
    {
    }
}
