namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class BlobClientNotFoundException : Exception
{
    public BlobClientNotFoundException(string argumentName)
        : base("Could not find blob client")
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; init; }
}
