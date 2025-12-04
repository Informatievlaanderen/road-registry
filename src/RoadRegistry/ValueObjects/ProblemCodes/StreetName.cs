namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class StreetName
    {
        public static readonly ProblemCode NotFound = new("StreetNameNotFound");
        public static readonly ProblemCode RegistryUnexpectedError = new("StreetNameRegistryUnexpectedError");
    }
}
