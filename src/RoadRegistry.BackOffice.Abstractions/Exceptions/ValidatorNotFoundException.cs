namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class ValidatorNotFoundException : Exception
{
    public ValidatorNotFoundException(string argumentName)
        : base("Could not resolve requested validator")
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; init; }
}
