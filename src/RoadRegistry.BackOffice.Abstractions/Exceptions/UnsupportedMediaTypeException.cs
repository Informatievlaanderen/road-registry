namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;
using Be.Vlaanderen.Basisregisters.BlobStore;

[Serializable]
public sealed class UnsupportedMediaTypeException : ApplicationException
{
    public ContentType? ContentType { get; }

    public UnsupportedMediaTypeException()
    {
    }

    public UnsupportedMediaTypeException(ContentType contentType)
    {
        ContentType = contentType;
    }

    private UnsupportedMediaTypeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public UnsupportedMediaTypeException(string? message) : base(message)
    {
    }
}
