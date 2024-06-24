namespace RoadRegistry.Integration.Schema.RoadSegments
{
    using System;

    public class RoadSegmentNumberedRoadAttributeLatestItem
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public string Number { get; set; }
        public int DirectionId { get; set; }
        public string DirectionLabel { get; set; }
        public int SequenceNumber { get; set; }
        public bool IsRemoved { get; set; }
        public string BeginOrganizationId { get; set; }
        public string BeginOrganizationName { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }
        public DateTimeOffset CreatedOnTimestamp { get; set; }
    }
}
