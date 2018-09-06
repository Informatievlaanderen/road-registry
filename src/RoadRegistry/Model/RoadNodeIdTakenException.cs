namespace RoadRegistry.Model
{
    public class RoadNodeIdTakenException : RoadRegistryException
    {
        public RoadNodeId Id { get; }

        public RoadNodeIdTakenException(RoadNodeId id)
            : base($"There's already a road node with id {id}.")
        {
            Id = id;
        }
    }
}
