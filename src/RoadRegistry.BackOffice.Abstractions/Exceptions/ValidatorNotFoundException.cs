namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class ValidatorNotFoundException : ApplicationException
{
    public ValidatorNotFoundException(string argumentName) : base("Could not resolve requested validator")
    {
        ArgumentName = argumentName;
    }
    
    private ValidatorNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public string ArgumentName { get; init; }
}
