namespace RoadRegistry.BackOffice.Configuration;
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
}
