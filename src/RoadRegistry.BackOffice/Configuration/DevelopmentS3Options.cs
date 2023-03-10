namespace RoadRegistry.BackOffice.Configuration;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Internal;
using Newtonsoft.Json;

public class DevelopmentS3Options : S3Options
{
    public DevelopmentS3Options(JsonSerializerSettings jsonSerializerSettings, string serviceUrl)
        : base(jsonSerializerSettings)
    {
        ServiceUrl = serviceUrl;
    }

    public override AmazonS3Client CreateS3Client()
    {
        return new AmazonS3Client(new BasicAWSCredentials("dummy", "dummy"), new AmazonS3Config()
        {
            ServiceURL = ServiceUrl,
            DisableHostPrefixInjection = true,
            ForcePathStyle = true,
            LogResponse = true
        });
    }
}
