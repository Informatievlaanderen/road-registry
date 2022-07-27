namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class UploadStatusNotFoundException : ApplicationException
{
    public UploadStatusNotFoundException(string? message) : base(message)
    {
    }
}
