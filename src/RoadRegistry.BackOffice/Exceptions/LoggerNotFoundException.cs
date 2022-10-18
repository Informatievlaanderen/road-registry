namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class LoggerNotFoundException<T> : ApplicationException
    where T : class
{
    public LoggerNotFoundException()
        : base($"Could not resolve required logger instance for type {typeof(T).Name}")
    { }

    protected LoggerNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}
