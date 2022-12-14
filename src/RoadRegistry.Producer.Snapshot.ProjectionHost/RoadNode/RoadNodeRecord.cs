namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using NetTopologySuite.Geometries;
    using Schema;

    public class RoadNodeRecord
    {
        public int Id { get; set; }

        public int TypeId { get; set; }
        public string TypeDutchName { get; set; }
        public Geometry Geometry { get; set; }

        public Origin Origin { get; set; }
        public DateTimeOffset LastChangedTimestamp { get; set; }
        public bool IsRemoved { get; set; }

        // EF needs this
        private RoadNodeRecord() { }

        public RoadNodeRecord(
            int id,
            int typeId,
            string typeDutchName,
            Point point,
            Origin origin,
            DateTimeOffset lastChangedTimestamp)
        {
            Id = id;
            Origin = origin;
            TypeId = typeId;
            TypeDutchName = typeDutchName;
            Geometry = point;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = false;
        }

        public RoadNodeSnapshot ToContract()
        {
            return new RoadNodeSnapshot(
                Id,
                TypeId,
                TypeDutchName,
                Geometry.ToBinary(),
                Geometry.ToText(),
                Geometry.SRID,
                Origin,
                LastChangedTimestamp,
                IsRemoved);
        }
    }
}
