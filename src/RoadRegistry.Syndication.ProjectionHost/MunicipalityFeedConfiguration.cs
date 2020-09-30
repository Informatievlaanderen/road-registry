namespace RoadRegistry.Syndication.ProjectionHost
{
    using System;

    public interface ISyndicationFeedConfiguration
    {
        public string UserName { get; }
        public string Password { get; }
        public Uri Uri { get; }
    }

    public class MunicipalityFeedConfiguration : ISyndicationFeedConfiguration
    {
        public const string Section = "MunicipalityFeed";

        public string UserName { get; set; }
        public string Password { get; set; }
        public Uri Uri { get; set; }
    }

    public class StreetNameFeedConfiguration : ISyndicationFeedConfiguration
    {
        public const string Section = "StreetNameFeed";

        public string UserName { get; set; }
        public string Password { get; set; }
        public Uri Uri { get; set; }
    }
}
