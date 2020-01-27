namespace RoadRegistry.BackOffice.Core
{
    public class RoadNodeGeometryMismatchException : RoadRegistryException
    {
        public RoadNodeId Id { get; }

        public RoadNodeGeometryMismatchException(RoadNodeId id)
            : base($"The road node {id} does not have a geometry of type PointM.")
        {
            Id = id;
        }
    }
}
