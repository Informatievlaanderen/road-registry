namespace RoadRegistry.Producer.Snapshot.ProjectionHost.NationalRoad
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using NetTopologySuite.Geometries;
    using Schema;

    public class NationalRoadRecord
    {
        public int Id { get; set; } // NW_OIDN
        public int RoadSegmentId { get; set; } // WS_OIDN
        public string Number { get; set; } // IDENT2

        public Origin Origin { get; set; }
        public DateTimeOffset LastChangedTimestamp { get; set; }
        public bool IsRemoved { get; set; }

        // EF needs this
        private NationalRoadRecord() { }

        public NationalRoadRecord(
            int id,
            int roadSegmentId,
            string number,
            DateTime? beginTime,
            string organization,
            DateTimeOffset lastChangedTimestamp)
        {
            Id = id;
            RoadSegmentId = roadSegmentId;
            Number = number;

            Origin = new Origin
            {
                Organization = organization,
                BeginTime = beginTime
            };
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = false;
        }

        public NationalRoadSnapshot ToContract()
        {
            return new NationalRoadSnapshot(
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
