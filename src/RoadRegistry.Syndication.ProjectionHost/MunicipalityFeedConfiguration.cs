namespace RoadRegistry.Syndication.ProjectionHost
{
    using System;

    public interface ISyndicationFeedConfiguration
    {
        public Uri Uri { get; }
    }

    public class MunicipalityFeedConfiguration : ISyndicationFeedConfiguration
    {
        public const string Section = "MunicipalityFeed";

        public Uri Uri { get; set; }
    }

    public class StreetNameFeedConfiguration : ISyndicationFeedConfiguration
    {
        public const string Section = "StreetNameFeed";

        public Uri Uri { get; set; }
    }
}
