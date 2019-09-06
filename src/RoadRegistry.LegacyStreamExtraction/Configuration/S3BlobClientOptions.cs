namespace RoadRegistry.LegacyStreamExtraction.Configuration
{
    public class S3BlobClientOptions
    {
        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string OutputBucket { get; set; }
    }
}