namespace RoadRegistry.Wfs.Schema
{
    using System;

    public class GradeSeparatedJunctionRecord
    {
        public int Id { get; set; }
        public DateTime? BeginTime { get; set; }
        public string Type { get; set; }
        public int? LowerRoadSegmentId { get; set; }
        public int? UpperRoadSegmentId { get; set; }
    }
}
