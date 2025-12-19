namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class NationalRoad
    {
        public static readonly ProblemCode NumberNotFound = new("NationalRoadNumberNotFound");
    }
}
