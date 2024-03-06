namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class Extract
    {
        public static readonly ProblemCode NotFound = new("ExtractNotFound");
    }
}
