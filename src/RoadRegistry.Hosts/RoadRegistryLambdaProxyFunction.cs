namespace RoadRegistry.Hosts;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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
        using (var httpClient = new HttpClient { BaseAddress = new Uri(_serviceUrl) })
        {
            var serializedEvent = JsonConvert.SerializeObject(@event, EventsJsonSerializerSettingsProvider.CreateSerializerSettings());
            var requestUri = $"{_serviceUrl.TrimEnd('/')}/runtime/invoke-event?configfile={_configFile}&functionhandler={_functionHandler}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(serializedEvent),
            };

            context.Logger.LogInformation("### LAMBDA PROXY REQUEST ###");
            context.Logger.LogInformation(JsonConvert.SerializeObject(request, new JsonSerializerSettings { Formatting = Formatting.Indented }));

            context.Logger.LogInformation("### LAMBDA PROXY SERIALIZED EVENT ###");
            context.Logger.LogInformation(JsonConvert.SerializeObject(@event, new JsonSerializerSettings { Formatting = Formatting.Indented }));

            var response = await httpClient.SendAsync(request);

            context.Logger.LogInformation("### LAMBDA PROXY RESPONSE ###");
            context.Logger.LogInformation(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }
    }
}
