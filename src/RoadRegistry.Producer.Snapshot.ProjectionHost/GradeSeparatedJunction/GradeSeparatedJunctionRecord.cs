namespace RoadRegistry.Producer.Snapshot.ProjectionHost.GradeSeparatedJunction
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using Schema;

    public class GradeSeparatedJunctionRecord
    {
        public int Id { get; set; }
        public int LowerRoadSegmentId { get; set; }
        public int UpperRoadSegmentId { get; set; }
        public int TypeId { get; set; }
        public string TypeDutchName { get; set; }

        public Origin Origin { get; set; }
        public DateTimeOffset LastChangedTimestamp { get; set; }
        public bool IsRemoved { get; set; }
        
        public GradeSeparatedJunctionSnapshot ToContract()
        {
            return new GradeSeparatedJunctionSnapshot(
                Id,
                LowerRoadSegmentId,
                UpperRoadSegmentId,
                TypeId,
                TypeDutchName,
                Origin,
                LastChangedTimestamp,
                IsRemoved
            );
        }
    }
}
