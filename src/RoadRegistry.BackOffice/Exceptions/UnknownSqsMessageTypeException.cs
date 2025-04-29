namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class UnknownSqsMessageTypeException : Exception
{
    public UnknownSqsMessageTypeException(string message, string messageType)
        : base(message)
    {
        MessageType = messageType;
    }

    public string MessageType { get; }
}
