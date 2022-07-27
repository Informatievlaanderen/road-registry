namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class LoggerNotFoundException<T> : ApplicationException
    where T : class
{
    public LoggerNotFoundException() : base($"Could not resolve required logger instance for type {typeof(T).Name}")
    {
    }
}
