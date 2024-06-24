namespace RoadRegistry.Integration.Schema.Organizations
{
    using System;

    public class OrganizationLatestItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string OvoCode { get; set; }
        public bool IsRemoved { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }
        public DateTimeOffset CreatedOnTimestamp { get; set; }
    }
}
