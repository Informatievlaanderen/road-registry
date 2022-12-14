namespace Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry
{
    using System;
    using global::RoadRegistry.Producer.Snapshot.ProjectionHost.Schema;

    public class GradeSeparatedJunctionSnapshot : IQueueMessage
    {
        public int Id { get; set; }
        public int LowerRoadSegmentId { get; set; }
        public int UpperRoadSegmentId { get; set; }
        public int TypeId { get; set; }
        public string TypeDutchName { get; set; }

        public Origin Origin { get; }
        public DateTimeOffset LastChangedTimestamp { get; }
        public bool IsRemoved { get; }

        public GradeSeparatedJunctionSnapshot(
            int id,
            int lowerRoadSegmentId,
            int upperRoadSegmentId,
            int typeId,
            string typeDutchName,
            Origin origin,
            DateTimeOffset lastChangedTimestamp,
            bool isRemoved)
        {
            Id = id;
            LowerRoadSegmentId = lowerRoadSegmentId;
            UpperRoadSegmentId = upperRoadSegmentId;
            TypeId = typeId;
            TypeDutchName = typeDutchName;

            Origin = origin;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = isRemoved;
        }
    }
}
