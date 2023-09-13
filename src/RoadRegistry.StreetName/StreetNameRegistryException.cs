namespace RoadRegistry.StreetName;

using System.Net;
using System.Runtime.Serialization;
using BackOffice.Exceptions;

public class StreetNameRegistryException : RoadRegistryException
{
    public StreetNameRegistryException(HttpStatusCode statusCode)
        : base($"A problem with the streetname registry occurred [{(int)statusCode}]")
    {
    }

    protected StreetNameRegistryException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
