namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class SqsQueuePublisherNotFoundException : Exception
{
    public SqsQueuePublisherNotFoundException(string argumentName)
        : base("Could not resolve SQS queue publisher")
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; init; }
}
