namespace RoadRegistry.Integration.Schema.RoadSegments.Version
{
    using System;

    public class RoadSegmentSurfaceAttributeVersion
    {
        public required long Position { get; init; }
        public required int Id { get; init; }
        public required int RoadSegmentId { get; init; }

        public int AsOfGeometryVersion { get; set; }
        public double FromPosition { get; set; }
        public double ToPosition { get; set; }
        public int TypeId { get; set; }
        public string TypeLabel { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public bool IsRemoved { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }
        public DateTimeOffset CreatedOnTimestamp { get; set; }

        public RoadSegmentSurfaceAttributeVersion Clone(long newPosition)
        {
            var newItem = new RoadSegmentSurfaceAttributeVersion
            {
                Position = newPosition,
                Id = Id,
                RoadSegmentId = RoadSegmentId,

                AsOfGeometryVersion = AsOfGeometryVersion,
                FromPosition = FromPosition,
                ToPosition = ToPosition,
                TypeId = TypeId,
                TypeLabel = TypeLabel,
                OrganizationId = OrganizationId,
                OrganizationName = OrganizationName,
                IsRemoved = IsRemoved,

                VersionTimestamp = VersionTimestamp,
                CreatedOnTimestamp = CreatedOnTimestamp
            };

            return newItem;
        }
    }
}
