namespace RoadRegistry.Integration.Schema.Organizations.Version
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using NodaTime;

    public class OrganizationVersion
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
        public const string CreatedOnTimestampBackingPropertyName = nameof(CreatedOnTimestampAsDateTimeOffset);

        public required long Position { get; init; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string OvoCode { get; set; }
        public string KboNumber { get; set; }
        public bool IsMaintainer { get; set; }
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

        public OrganizationVersion CloneAndApplyEventInfo(
            long newPosition,
            Action<OrganizationVersion> applyEventInfo)
        {
            var newVersion = new OrganizationVersion
            {
                Position = newPosition,
                Code = Code,
                Name = Name,
                OvoCode = OvoCode,
                KboNumber = KboNumber,
                IsMaintainer = IsMaintainer,
                IsRemoved = IsRemoved,
                VersionTimestamp = VersionTimestamp,
                CreatedOnTimestamp = CreatedOnTimestamp
            };

            applyEventInfo(newVersion);

            return newVersion;
        }
    }
}
