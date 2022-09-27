using System;

namespace RoadRegistry.BackOffice.Exceptions
{
    public class UnknownSqsMessageTypeException : ApplicationException
    {
        public UnknownSqsMessageTypeException(string message, string messageType) : base(message)
        {
            MessageType = messageType;
        }

        public string MessageType { get; }
    }
}
