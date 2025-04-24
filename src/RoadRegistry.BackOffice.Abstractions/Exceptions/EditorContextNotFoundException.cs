namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class EditorContextNotFoundException : Exception
{
    public EditorContextNotFoundException(string argumentName)
        : base("Could not resolve an editor context")
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; init; }
}
