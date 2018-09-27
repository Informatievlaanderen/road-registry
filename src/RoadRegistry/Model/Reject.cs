namespace RoadRegistry.Model
{
    public static class Reject
    {
        public static IRoadNetworkChangeRejection AddRoadNode(RoadNodeId id, string reason)
        {
            return new AddRoadNodeRejection(id, reason);
        }
    }
}
