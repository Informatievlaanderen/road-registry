namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class UnknownSqsMessageTypeException : ApplicationException
{
    public UnknownSqsMessageTypeException(string message, string messageType)
        : base(message)
    {
        MessageType = messageType;
    }

    protected UnknownSqsMessageTypeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string MessageType { get; }
}