namespace RoadRegistry.Syndication.ProjectionHost
{
    using System;

    public class MunicipalityFeedConfiguration
    {
        public const string Section = "MunicipalityFeed";

        public string UserName { get; set; }
        public string Password { get; set; }
        public Uri Uri { get; set; }
    }
}
