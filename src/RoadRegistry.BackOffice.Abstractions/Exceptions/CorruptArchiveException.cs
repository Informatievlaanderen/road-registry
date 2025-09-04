namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using BackOffice.Exceptions;

public sealed class CorruptArchiveException : RoadRegistryException
{
    public CorruptArchiveException()
    {
    }

    public CorruptArchiveException(string? message) : base(message)
    {
    }
}
