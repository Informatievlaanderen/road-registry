namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class RoadNetwork
    {
        public static ProblemCode NotFound = new("RoadNetworkNotFound");
    }
}
