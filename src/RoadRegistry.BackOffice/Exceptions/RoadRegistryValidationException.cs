namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class RoadRegistryValidationException : RoadRegistryException
{
    public string ErrorCode { get; }

    public RoadRegistryValidationException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected RoadRegistryValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
