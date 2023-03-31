namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class NumberedRoad
    {
        public static ProblemCode NumberNotFound = new("NumberedRoadNumberNotFound");
    }
}
