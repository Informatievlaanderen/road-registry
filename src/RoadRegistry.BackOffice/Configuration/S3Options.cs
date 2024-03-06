namespace RoadRegistry.BackOffice.Configuration;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Newtonsoft.Json;

public class S3Options : IHasConfigurationKey
{
    public string ServiceUrl { get; set; }

    public AWSCredentials Credentials { get; set; }
    public RegionEndpoint RegionEndpoint { get; }
    public JsonSerializerSettings JsonSerializerSettings { get; }

    public S3Options() {}

    public S3Options(JsonSerializerSettings jsonSerializerSettings)
        : this(null, jsonSerializerSettings)
    {
    }

    public S3Options(RegionEndpoint regionEndpoint, JsonSerializerSettings jsonSerializerSettings = null)
    {
        RegionEndpoint = regionEndpoint ?? RegionEndpoint.EUWest1;
        JsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings();
    }

    public S3Options(AWSCredentials credentials, RegionEndpoint regionEndpoint)
        : this(regionEndpoint)
    {
        Credentials = credentials;
    }

    public S3Options(string accessKey, string secretKey, RegionEndpoint regionEndpoint)
        : this(new BasicAWSCredentials(accessKey, secretKey), regionEndpoint)
    { }

    public S3Options(string accessKey, string secretKey, string sessionToken, RegionEndpoint regionEndpoint)
        : this(new SessionAWSCredentials(accessKey, secretKey, sessionToken), regionEndpoint)
    { }

    public virtual IAmazonS3 CreateS3Client()
    {
        var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint };
        return Credentials != null ? new AmazonS3Client(Credentials, config) : new AmazonS3Client(config);
    }

    public string GetConfigurationKey()
    {
        return "S3";
    }
}
