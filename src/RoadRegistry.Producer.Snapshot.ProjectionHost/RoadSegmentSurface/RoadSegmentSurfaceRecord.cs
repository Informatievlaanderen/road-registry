namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using Schema;

    public class RoadSegmentSurfaceRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public int RoadSegmentGeometryVersion { get; set; }
        public int TypeId { get; set; }
        public string TypeDutchName { get; set; }
        public double FromPosition { get; set; }
        public double ToPosition { get; set; }

        public Origin Origin { get; set; }
        public DateTimeOffset LastChangedTimestamp { get; set; }
        public bool IsRemoved { get; set; }

        // EF needs this
        private RoadSegmentSurfaceRecord() { }

        public RoadSegmentSurfaceRecord(
            int id,
            int roadSegmentId,
            int roadSegmentGeometryVersion,
            int typeId,
            string typeDutchName,
            double fromPosition,
            double toPosition,
            Origin origin,
            DateTimeOffset lastChangedTimestamp)
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
            IsRemoved = false;
        }

        public RoadSegmentSurfaceSnapshot ToContract()
        {
            return new RoadSegmentSurfaceSnapshot(
                Id,
                RoadSegmentId,
                RoadSegmentGeometryVersion,
                TypeId,
                TypeDutchName,
                FromPosition,
                ToPosition,
                Origin,
                LastChangedTimestamp,
                IsRemoved
            );
        }
    }
}
