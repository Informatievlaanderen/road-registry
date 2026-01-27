namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class RoadNetwork
    {
        public static readonly ProblemCode NotFound = new("RoadNetworkNotFound");
        public static readonly ProblemCode Disconnected = new("RoadNetworkDisconnected");
        public static readonly ProblemCode NoChanges = new("RoadNetworkNoChanges");
    }
}
