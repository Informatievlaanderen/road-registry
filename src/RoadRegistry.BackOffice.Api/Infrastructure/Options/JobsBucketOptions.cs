namespace RoadRegistry.BackOffice.Api.Infrastructure.Options
{
    public class JobsBucketOptions
    {
        public const string ConfigKey = "JobsBucket";

        public string BucketName { get; set; }
        public int UrlExpirationInMinutes { get; set; }
    }
}
