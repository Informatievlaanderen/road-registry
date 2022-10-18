namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class SqsOptionsNotFoundException : ApplicationException
{
    public SqsOptionsNotFoundException(string argumentName)
        : base("Could not resolve SQS options")
    {
        ArgumentName = argumentName;
    }

    protected SqsOptionsNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string ArgumentName { get; init; }
}