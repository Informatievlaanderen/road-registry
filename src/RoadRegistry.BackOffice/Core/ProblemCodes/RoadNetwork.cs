namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class RoadNetwork
    {
        public static readonly ProblemCode NotFound = new("RoadNetworkNotFound");
        public static readonly ProblemCode Disconnected = new("RoadNetworkDisconnected");
    }
}
