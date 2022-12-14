namespace Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry
{
    using System;
    using global::RoadRegistry.Producer.Snapshot.ProjectionHost.Schema;

    public class RoadSegmentSurfaceSnapshot : IQueueMessage
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public int RoadSegmentGeometryVersion { get; set; }
        public int TypeId { get; set; }
        public string TypeDutchName { get; set; }
        public double FromPosition { get; set; }
        public double ToPosition { get; set; }

        public Origin Origin { get; }
        public DateTimeOffset LastChangedTimestamp { get; }
        public bool IsRemoved { get; }

        public RoadSegmentSurfaceSnapshot(
            int id,
            int roadSegmentId,
            int roadSegmentGeometryVersion,
            int typeId,
            string typeDutchName,
            double fromPosition,
            double toPosition,
            Origin origin,
            DateTimeOffset lastChangedTimestamp,
            bool isRemoved)
        {
            Id = id;
            RoadSegmentId = roadSegmentId;
            RoadSegmentGeometryVersion = roadSegmentGeometryVersion;
            TypeId = typeId;
            TypeDutchName = typeDutchName;
            FromPosition = fromPosition;
            ToPosition = toPosition;

            Origin = origin;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = isRemoved;
        }
    }
}
