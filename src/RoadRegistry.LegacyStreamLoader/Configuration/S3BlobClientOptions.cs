namespace RoadRegistry.LegacyStreamLoader.Configuration
{
    public class S3BlobClientOptions
    {
        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string InputBucket { get; set; }
    }
}