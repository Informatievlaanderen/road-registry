namespace RoadRegistry.BackOffice.Api.Infrastructure.Options
{
    public class JobsBucketOptions
    {
        public const string ConfigKey = "JobsBucket";

        public string BucketName { get; set; }
        public int UrlExpirationInMinutes { get; set; }

        // The portal uploads the file the moment it receives the upload url, so a job that has not received a file
        // within these minutes never will: it no longer counts as an upload in progress for its extract.
        public int PendingUploadTimeoutInMinutes { get; set; } = 5;
    }
}
