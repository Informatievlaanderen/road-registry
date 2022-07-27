namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class UnsupportedMediaTypeException : ApplicationException
{
    public UnsupportedMediaTypeException()
    {
    }

    public UnsupportedMediaTypeException(string? message) : base(message)
    {
    }
}
