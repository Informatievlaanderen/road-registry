namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class NumberedRoad
    {
        public static readonly ProblemCode NumberNotFound = new("NumberedRoadNumberNotFound");
    }
}
