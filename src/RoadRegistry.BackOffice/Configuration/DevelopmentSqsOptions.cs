namespace RoadRegistry.BackOffice.Configuration;

using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Newtonsoft.Json;

public class DevelopmentSqsOptions : SqsOptions
{
    private readonly string _serviceUrl;

    public DevelopmentSqsOptions()
    {
    }

    public DevelopmentSqsOptions(JsonSerializerSettings jsonSerializerSettings, string serviceUrl)
        : base(
            regionEndpoint: RegionEndpoint.EUWest1,
            jsonSerializerSettings: jsonSerializerSettings
        )
    {
        _serviceUrl = serviceUrl;
    }

    public override AmazonSQSClient CreateSqsClient()
    {
        return new AmazonSQSClient(new BasicAWSCredentials("dummy", "dummy"), new AmazonSQSConfig()
        {
            ServiceURL = _serviceUrl
        });
    }
}
