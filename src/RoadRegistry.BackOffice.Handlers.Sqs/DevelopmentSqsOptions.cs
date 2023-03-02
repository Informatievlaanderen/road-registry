namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Amazon.Runtime;
using Amazon.SQS;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Newtonsoft.Json;

public class DevelopmentSqsOptions : SqsOptions
{
    private readonly string _serviceUrl;

    public DevelopmentSqsOptions(JsonSerializerSettings jsonSerializerSettings, string serviceUrl)
        : base(jsonSerializerSettings)
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
