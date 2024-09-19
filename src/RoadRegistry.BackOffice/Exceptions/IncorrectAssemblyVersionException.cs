namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class IncorrectAssemblyVersionException : RoadRegistryException
{
    public IncorrectAssemblyVersionException(string incorrectVersion) : base($"Assembly version '{incorrectVersion}' is incorrect.")
    {
    }

    protected IncorrectAssemblyVersionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

}
