namespace Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry
{
    using System;

    public class GradeSeparatedJunctionSnapshot : IQueueMessage
    {
        public int Id { get; set; }
        public int LowerRoadSegmentId { get; set; }
        public int UpperRoadSegmentId { get; set; }
        public int Type { get; set; }

        public DateTimeOffset? BeginTime { get; }
        public string Organization { get; }
        public DateTimeOffset LastChangedTimestamp { get; }
        public bool IsRemoved { get; }

        public GradeSeparatedJunctionSnapshot(
            int id,
            int lowerRoadSegmentId,
            int upperRoadSegmentId,
            int type,
            DateTimeOffset? beginTime,
            string organization,
            DateTimeOffset lastChangedTimestamp,
            bool isRemoved)
        {
            Id = id;
            LowerRoadSegmentId = lowerRoadSegmentId;
            UpperRoadSegmentId = upperRoadSegmentId;
            Type = type;
            BeginTime = beginTime;
            Organization = organization;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = isRemoved;
        }
    }
}
