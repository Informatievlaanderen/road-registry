namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class GradeJunction
    {
        public static readonly ProblemCode NotFound = new("GradeJunctionNotFound");
    }
}
