namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class SqsQueueConsumerNotFoundException : Exception
{
    public SqsQueueConsumerNotFoundException(string argumentName)
        : base("Could not resolve SQS queue consumer")
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; init; }
}
