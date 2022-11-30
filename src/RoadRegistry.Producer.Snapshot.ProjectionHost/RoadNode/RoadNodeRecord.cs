namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using NetTopologySuite.Geometries;
    using Schema;

    public class RoadNodeRecord
    {
        public int Id { get; set; }

        public string Type { get; set; }
        public Geometry Geometry { get; set; }

        public Origin Origin { get; set; }
        public DateTimeOffset LastChangedTimestamp { get; set; }
        public bool IsRemoved { get; set; }

        // EF needs this
        private RoadNodeRecord() { }

        public RoadNodeRecord(
            int id,
            string type,
            Point point,
            DateTime? beginTime,
            string organization,
            DateTimeOffset lastChangedTimestamp)
        {
            Id = id;
            Origin = new Origin
            {
                Organization = organization,
                BeginTime = beginTime
            };
            Type = type;
            Geometry = point;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = false;
        }

        public RoadNodeSnapshot ToContract()
        {
            return new RoadNodeSnapshot(
                Id,
                Type,
                Geometry.ToBinary(),
                Geometry.ToText(),
                Geometry.SRID,
                Origin.BeginTime,
                Origin.Organization,
                LastChangedTimestamp,
                IsRemoved);
        }
    }
}
