namespace Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry
{
    using System;

    public class NationalRoadSnapshot : IQueueMessage
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public string Number { get; set; }

        public DateTimeOffset? BeginTime { get; }
        public string Organization { get; }
        public DateTimeOffset LastChangedTimestamp { get; }
        public bool IsRemoved { get; }

        public NationalRoadSnapshot(
            int id,
            int roadSegmentId,
            string number,
            DateTimeOffset? beginTime,
            string organization,
            DateTimeOffset lastChangedTimestamp,
            bool isRemoved)
        {
            Id = id;
            RoadSegmentId = roadSegmentId;
            Number = number;
            BeginTime = beginTime;
            Organization = organization;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = isRemoved;
        }
    }
}
