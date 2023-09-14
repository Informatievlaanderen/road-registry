namespace RoadRegistry.StreetName;

using System;
using System.Net;
using System.Runtime.Serialization;
using BackOffice.Exceptions;

[Serializable]
public class StreetNameRegistryUnexpectedStatusCodeException : RoadRegistryException
{
    public HttpStatusCode StatusCode { get; }

    public StreetNameRegistryUnexpectedStatusCodeException(HttpStatusCode statusCode)
        : base($"An unexpected error {(int)statusCode} with streetname registry occurred")
    {
        StatusCode = statusCode;
    }

    protected StreetNameRegistryUnexpectedStatusCodeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
