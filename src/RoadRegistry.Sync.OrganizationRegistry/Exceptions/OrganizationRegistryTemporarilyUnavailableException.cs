namespace RoadRegistry.Sync.OrganizationRegistry.Exceptions;

using System;
using System.Runtime.Serialization;
using RoadRegistry.BackOffice.Exceptions;

[Serializable]
public class OrganizationRegistryTemporarilyUnavailableException : RoadRegistryException
{
    public OrganizationRegistryTemporarilyUnavailableException()
    {
    }

    protected OrganizationRegistryTemporarilyUnavailableException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
