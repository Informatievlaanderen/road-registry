namespace RoadRegistry.Integration.Schema.RoadSegments.Version
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using NodaTime;

    public class RoadSegmentSurfaceAttributeVersion
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
        public const string CreatedOnTimestampBackingPropertyName = nameof(CreatedOnTimestampAsDateTimeOffset);

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

        public string VersionAsString { get; set; }
        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set
            {
                VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

        public string CreatedOnAsString { get; set; }
        private DateTimeOffset CreatedOnTimestampAsDateTimeOffset { get; set; }

        public Instant CreatedOnTimestamp
        {
            get => Instant.FromDateTimeOffset(CreatedOnTimestampAsDateTimeOffset);
            set
            {
                CreatedOnTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                CreatedOnAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

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
