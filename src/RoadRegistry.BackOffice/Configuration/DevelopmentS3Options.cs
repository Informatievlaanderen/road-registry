namespace RoadRegistry.BackOffice.Configuration;

using Amazon.Runtime;
using Amazon.S3;
using Newtonsoft.Json;

public class DevelopmentS3Options : S3Options
{
    public DevelopmentS3Options()
    {
    }

    public DevelopmentS3Options(JsonSerializerSettings jsonSerializerSettings, DevelopmentS3Options options)
        : base(jsonSerializerSettings)
    {
        ServiceUrl = options.ServiceUrl;
        AccessKey = options.AccessKey;
        SecretKey = options.SecretKey;
    }

    public string AccessKey { get; set; }
    public string SecretKey { get; set; }

    public override IAmazonS3 CreateS3Client()
    {
        return new AmazonS3Client(new BasicAWSCredentials(AccessKey ?? "dummy", SecretKey ?? "dummy"), new AmazonS3Config
        {
            ServiceURL = ServiceUrl,
            DisableHostPrefixInjection = true,
            ForcePathStyle = true,
            LogResponse = true
        });
    }
}
