namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class ValidatorNotFoundException : ApplicationException
{
    public ValidatorNotFoundException(string argumentName) : base("Could not resolve requested validator")
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; init; }
}
