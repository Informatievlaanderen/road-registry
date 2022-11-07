namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class BlobClientNotFoundException : ApplicationException
{
    public BlobClientNotFoundException(string argumentName) : base("Could not find blob client")
    {
        ArgumentName = argumentName;
    }

    private BlobClientNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string ArgumentName { get; init; }
}
