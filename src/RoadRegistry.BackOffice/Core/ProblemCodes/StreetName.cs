namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class StreetName
    {
        public static ProblemCode NotFound = new("StreetNameNotFound");
    }
}
