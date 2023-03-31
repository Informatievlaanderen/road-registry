namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class GradeSeparatedJunction
    {
        public static ProblemCode NotFound = new("GradeSeparatedJunctionNotFound");
    }
}
