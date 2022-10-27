namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class UnsupportedMediaTypeException : ApplicationException
{
    public UnsupportedMediaTypeException()
    {
    }

    private UnsupportedMediaTypeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public UnsupportedMediaTypeException(string? message) : base(message)
    {
    }
}