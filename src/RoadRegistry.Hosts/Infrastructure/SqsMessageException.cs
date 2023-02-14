namespace RoadRegistry.Hosts.Infrastructure;

using System;

public class SqsMessageException : ApplicationException
{
    public SqsMessageException(string message) : base(message)
    {
    }
}
