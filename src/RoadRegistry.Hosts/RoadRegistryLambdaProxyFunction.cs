using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace RoadRegistry.Hosts
{
    using System;
    using Amazon.Lambda;
    using Amazon.Lambda.Model;

    public abstract class RoadRegistryLambdaProxyFunction
    {
        private readonly AmazonLambdaClient _lambdaClient;

        protected RoadRegistryLambdaProxyFunction(string serviceUrl)
        {
            _lambdaClient = new AmazonLambdaClient(
                new BasicAWSCredentials("dummy", "dummy"),
                new AmazonLambdaConfig()
                {
                    ServiceURL = serviceUrl,
                    
                }
            );
        }

        public async Task FunctionHandler(SQSEvent @event, ILambdaContext context)
        {
            try
            {
                var functions = await _lambdaClient.ListFunctionsAsync();

                var request = new InvokeRequest
                {
                    FunctionName = "FunctionHandler",
                    InvocationType = InvocationType.RequestResponse,
                    LogType = LogType.Tail,
                    Payload = JsonConvert.SerializeObject(@event, EventsJsonSerializerSettingsProvider.CreateSerializerSettings())
                };
                await _lambdaClient.InvokeAsync(request);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
