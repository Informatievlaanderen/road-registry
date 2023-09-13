namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class StreetName
    {
        public static readonly ProblemCode NotFound = new("StreetNameNotFound");
        public static readonly ProblemCode RegistryUnknownError = new("StreetNameRegistryUnknownError");
    }
}
