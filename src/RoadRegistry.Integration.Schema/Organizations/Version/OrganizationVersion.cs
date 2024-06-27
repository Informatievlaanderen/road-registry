namespace RoadRegistry.Integration.Schema.Organizations.Version
{
    using System;

    public class OrganizationVersion
    {
        public required long Position { get; init; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string OvoCode { get; set; }
        public bool IsRemoved { get; set; }
        public DateTimeOffset VersionTimestamp { get; set; }
        public DateTimeOffset CreatedOnTimestamp { get; set; }

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
                IsRemoved = IsRemoved,
                VersionTimestamp = VersionTimestamp,
                CreatedOnTimestamp = CreatedOnTimestamp
            };

            applyEventInfo(newVersion);

            return newVersion;
        }
    }
}
