namespace RoadRegistry.Producer.Snapshot.ProjectionHost.NationalRoad
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using Schema;

    public class NationalRoadRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public string Number { get; set; }

        public Origin Origin { get; set; }
        public DateTimeOffset LastChangedTimestamp { get; set; }
        public bool IsRemoved { get; set; }

        // EF needs this
        private NationalRoadRecord() { }

        public NationalRoadRecord(
            int id,
            int roadSegmentId,
            string number,
            Origin origin,
            DateTimeOffset lastChangedTimestamp)
        {
            Id = id;
            RoadSegmentId = roadSegmentId;
            Number = number;

            Origin = origin;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = false;
        }

        public NationalRoadSnapshot ToContract()
        {
            return new NationalRoadSnapshot(
                Id,
                RoadSegmentId,
                Number,
                Origin,
                LastChangedTimestamp,
                IsRemoved
            );
        }
    }
}
