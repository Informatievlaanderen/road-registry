namespace Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry
{
    using System;
    using global::RoadRegistry.Producer.Snapshot.ProjectionHost.Schema;

    public class NationalRoadSnapshot : IQueueMessage
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public string Number { get; set; }

        public Origin Origin { get; }
        public DateTimeOffset LastChangedTimestamp { get; }
        public bool IsRemoved { get; }

        public NationalRoadSnapshot(
            int id,
            int roadSegmentId,
            string number,
            Origin origin,
            DateTimeOffset lastChangedTimestamp,
            bool isRemoved)
        {
            Id = id;
            RoadSegmentId = roadSegmentId;
            Number = number;

            Origin = origin;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = isRemoved;
        }
    }
}
