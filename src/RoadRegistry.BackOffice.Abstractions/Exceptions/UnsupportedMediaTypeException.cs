namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using BackOffice.Exceptions;
using Be.Vlaanderen.Basisregisters.BlobStore;

public sealed class UnsupportedMediaTypeException : RoadRegistryException
{
    public ContentType? ContentType { get; }

    public UnsupportedMediaTypeException()
    {
    }

    public UnsupportedMediaTypeException(ContentType contentType)
    {
        ContentType = contentType;
    }

    public UnsupportedMediaTypeException(string? message) : base(message)
    {
    }
}
