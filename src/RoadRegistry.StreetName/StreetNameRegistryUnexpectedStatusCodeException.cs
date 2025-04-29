namespace RoadRegistry.StreetName;

using System.Net;
using BackOffice.Exceptions;

public class StreetNameRegistryUnexpectedStatusCodeException : RoadRegistryException
{
    public HttpStatusCode StatusCode { get; }

    public StreetNameRegistryUnexpectedStatusCodeException(HttpStatusCode statusCode)
        : base($"An unexpected error {(int)statusCode} with streetname registry occurred")
    {
        StatusCode = statusCode;
    }
}
