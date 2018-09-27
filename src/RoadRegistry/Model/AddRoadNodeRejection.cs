namespace RoadRegistry.Model
{
    public class AddRoadNodeRejection : IRoadNetworkChangeRejection
    {
        public AddRoadNodeRejection(
            RoadNodeId id,
            string reason)
        {
            Id = id;
            Reason = reason;
        }

        public RoadNodeId Id { get; }
        public string Reason { get; }
    }
}
