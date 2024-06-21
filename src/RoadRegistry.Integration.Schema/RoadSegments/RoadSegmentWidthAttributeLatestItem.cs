namespace RoadRegistry.Integration.Schema.RoadSegments
{
    using System;

    public class RoadSegmentWidthAttributeLatestItem
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public int AsOfGeometryVersion { get; set; }
        public double FromPosition { get; set; }
        public double ToPosition { get; set; }
        public int Width { get; set; }
        public string WidthLabel { get; set; }
        public string BeginOrganizationId { get; set; }
        public string BeginOrganizationName { get; set; }
        public bool IsRemoved { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }
        public DateTimeOffset CreatedOnTimestamp { get; set; }
    }
}
