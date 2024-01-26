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
