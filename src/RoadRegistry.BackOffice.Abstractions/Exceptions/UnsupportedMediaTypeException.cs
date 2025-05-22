namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using Be.Vlaanderen.Basisregisters.BlobStore;

public sealed class UnsupportedMediaTypeException : Exception
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
