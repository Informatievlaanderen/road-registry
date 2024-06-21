namespace RoadRegistry.Integration.Schema.RoadSegments
{
    using System;

    public class RoadSegmentLaneAttributeLatestItem
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public int Version { get; set; }
        public int Count { get; set; }
        public int DirectionId { get; set; }
        public string DirectionLabel { get; set; }
        public double FromPosition { get; set; }
        public double ToPosition { get; set; }
        public string BeginOrganizationId { get; set; }
        public string BeginOrganizationName { get; set; }
        public bool IsRemoved { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }
        public DateTimeOffset CreatedOnTimestamp { get; set; }
    }
}
