namespace RoadRegistry.BackOffice.Exceptions;

public class IncorrectAssemblyVersionException : RoadRegistryException
{
    public IncorrectAssemblyVersionException(string incorrectVersion)
        : base($"Assembly version '{incorrectVersion}' is incorrect.")
    {
    }
}
