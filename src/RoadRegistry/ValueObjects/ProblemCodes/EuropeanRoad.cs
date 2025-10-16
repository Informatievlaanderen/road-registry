namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class EuropeanRoad
    {
        public static readonly ProblemCode NumberNotFound = new("EuropeanRoadNumberNotFound");
    }
}
