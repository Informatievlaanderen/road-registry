namespace RoadRegistry.BackOffice.Configuration;
using Newtonsoft.Json;

public class MinioS3Options : S3Options
{
    public MinioS3Options()
    {
    }

    public MinioS3Options(JsonSerializerSettings jsonSerializerSettings, MinioS3Options options)
        : base(jsonSerializerSettings)
    {
        ServiceUrl = options.ServiceUrl;
        AccessKey = options.AccessKey;
        SecretKey = options.SecretKey;
    }

    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
}
