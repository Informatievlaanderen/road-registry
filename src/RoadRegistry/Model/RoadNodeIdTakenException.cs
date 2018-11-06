namespace RoadRegistry.Model
{
    using Aiv.Vbr.Shaperon;

    public class RoadNodeIdTakenException : RoadRegistryException
    {
        public RoadNodeId Id { get; }

        public RoadNodeIdTakenException(RoadNodeId id)
            : base($"There's already a road node with id {id}.")
        {
            Id = id;
        }
    }

    public class RoadNodeGeometryTakenException : RoadRegistryException
    {
        public RoadNodeId Id { get; }
        public RoadNodeId ConflictsWithId { get; }
        public PointM Geometry { get; }

        public RoadNodeGeometryTakenException (RoadNodeId id, RoadNodeId conflictsWithId, PointM geometry)
            : base($"There's already a road node {conflictsWithId} that has the same geometry {geometry.AsText()} as {id}.")
        {
            Id = id;
            ConflictsWithId = conflictsWithId;
            Geometry = geometry;
        }
    }
}
