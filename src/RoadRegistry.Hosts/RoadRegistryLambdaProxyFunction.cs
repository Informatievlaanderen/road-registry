namespace RoadRegistry.Hosts;

using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Azure.Core;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Newtonsoft.Json;

public abstract class RoadRegistryLambdaProxyFunction
{
    private readonly string _configFile;
    private readonly string _functionHandler;
    private readonly string _serviceUrl;

    protected RoadRegistryLambdaProxyFunction(string serviceUrl, string functionHandler, string configFile)
    {
        _serviceUrl = serviceUrl;
        _functionHandler = functionHandler;
        _configFile = configFile;
    }

    public async Task FunctionHandler(SQSEvent @event, ILambdaContext context)
    {
        using (var client = new HttpClient())
        {
            var requestUriBuilder = new RequestUriBuilder();
            requestUriBuilder.AppendPath($"{_serviceUrl.TrimEnd('/')}/runtime/invoke-event");
            requestUriBuilder.AppendQuery("configfile", _configFile);
            requestUriBuilder.AppendQuery("functionhandler", _functionHandler);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUriBuilder.ToString())
            {
                Content = new StringContent(JsonConvert.SerializeObject(@event, EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
            };

            await client.SendAsync(request);
        }
    }
}
