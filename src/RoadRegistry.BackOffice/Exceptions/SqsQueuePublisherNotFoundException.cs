namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class SqsQueuePublisherNotFoundException : ApplicationException
{
    public SqsQueuePublisherNotFoundException(string argumentName)
        : base("Could not resolve SQS queue publisher")
    {
        ArgumentName = argumentName;
    }

    protected SqsQueuePublisherNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public string ArgumentName { get; init; }
}
