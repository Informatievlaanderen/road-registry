namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class UnsupportedMediaTypeException : ApplicationException
{
    public UnsupportedMediaTypeException()
    {
    }

    public UnsupportedMediaTypeException(string? message) : base(message)
    {
    }
}
