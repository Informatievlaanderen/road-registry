namespace RoadRegistry.Syndication.ProjectionHost
{
    using System;

    public class StreetNameFeedConfiguration : ISyndicationFeedConfiguration
    {
        public const string Section = "StreetNameFeed";

        public Uri Uri { get; set; }
    }
}
