namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class UploadExtractNotFoundException : ApplicationException
{
    public UploadExtractNotFoundException(string? message) : base(message)
    {
    }
}
