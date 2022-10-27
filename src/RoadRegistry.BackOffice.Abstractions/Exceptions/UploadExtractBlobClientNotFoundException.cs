namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class UploadExtractBlobClientNotFoundException : ApplicationException
{
    public UploadExtractBlobClientNotFoundException(string argumentName) : base("Could not find blob client for extract upload")
    {
        ArgumentName = argumentName;
    }

    private UploadExtractBlobClientNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string ArgumentName { get; init; }
}