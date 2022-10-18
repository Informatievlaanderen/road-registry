namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class SqsQueueConsumerNotFoundException : ApplicationException
{
    public SqsQueueConsumerNotFoundException(string argumentName)
        : base("Could not resolve SQS queue consumer")
    {
        ArgumentName = argumentName;
    }

    protected SqsQueueConsumerNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string ArgumentName { get; init; }
}
