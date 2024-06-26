namespace RoadRegistry.Integration.Schema.RoadSegments.Version
{
    using System;

    public class RoadSegmentLaneAttributeVersion
    {
        public required long Position { get; init; }
        public required int Id { get; init; }
        public required int RoadSegmentId { get; init; }

        public int AsOfGeometryVersion { get; set; }
        public double FromPosition { get; set; }
        public double ToPosition { get; set; }
        public int Count { get; set; }
        public int DirectionId { get; set; }
        public string DirectionLabel { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public bool IsRemoved { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }
        public DateTimeOffset CreatedOnTimestamp { get; set; }

        public RoadSegmentLaneAttributeVersion Clone(long newPosition)
        {
            var newItem = new RoadSegmentLaneAttributeVersion
            {
                Position = newPosition,
                Id = Id,
                RoadSegmentId = RoadSegmentId,

                AsOfGeometryVersion = AsOfGeometryVersion,
                FromPosition = FromPosition,
                ToPosition = ToPosition,
                Count = Count,
                DirectionId = DirectionId,
                DirectionLabel = DirectionLabel,
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
