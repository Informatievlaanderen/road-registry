namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class EditorContextNotFoundException : ApplicationException
{
    public EditorContextNotFoundException(string argumentName) : base("Could not resolve an editor context")
    {
        ArgumentName = argumentName;
    }
    
    private EditorContextNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public string ArgumentName { get; init; }
}
