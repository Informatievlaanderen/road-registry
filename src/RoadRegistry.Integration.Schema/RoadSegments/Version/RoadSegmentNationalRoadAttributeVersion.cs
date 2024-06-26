namespace RoadRegistry.Integration.Schema.RoadSegments.Version
{
    using System;

    public class RoadSegmentNationalRoadAttributeVersion
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public string Number { get; set; }
        public bool IsRemoved { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public DateTimeOffset VersionTimestamp { get; set; }
        public DateTimeOffset CreatedOnTimestamp { get; set; }
    }
}
