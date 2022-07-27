namespace RoadRegistry.BackOffice.Exceptions;

using System;

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
