namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class EditorContextNotFoundException : ApplicationException
{
    public EditorContextNotFoundException(string argumentName) : base("Could not resolve an editor context")
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; init; }
}
