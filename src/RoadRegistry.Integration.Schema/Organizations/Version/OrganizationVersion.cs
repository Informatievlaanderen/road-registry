namespace RoadRegistry.Integration.Schema.Organizations.Version
{
    using System;

    public class OrganizationVersion
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string OvoCode { get; set; }
        public bool IsRemoved { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }
        public DateTimeOffset CreatedOnTimestamp { get; set; }
    }
}
