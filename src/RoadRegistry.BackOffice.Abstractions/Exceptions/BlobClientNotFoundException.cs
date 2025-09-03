namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using BackOffice.Exceptions;

public sealed class BlobClientNotFoundException : RoadRegistryException
{
    public BlobClientNotFoundException(string argumentName)
        : base("Could not find blob client")
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; init; }
}
