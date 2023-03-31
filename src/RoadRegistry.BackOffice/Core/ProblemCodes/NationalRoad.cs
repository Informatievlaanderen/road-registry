namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class NationalRoad
    {
        public static ProblemCode NumberNotFound = new("NationalRoadNumberNotFound");
    }
}
