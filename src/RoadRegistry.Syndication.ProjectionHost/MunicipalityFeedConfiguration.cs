namespace RoadRegistry.Syndication.ProjectionHost
{
    using System;

    public class MunicipalityFeedConfiguration : ISyndicationFeedConfiguration
    {
        public const string Section = "MunicipalityFeed";

        public Uri Uri { get; set; }
    }
}
