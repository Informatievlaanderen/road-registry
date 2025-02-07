namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Collection("runsequential")]
public class FunctionTests
{
    [Fact]
    public async Task CanCreateFunction()
    {
        var function = new TestFunction();
        var json = JsonConvert.SerializeObject(new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>()
        });
        var jObject = JObject.Parse(json);

        await function.Handler(jObject, new TestLambdaContext());

        Assert.True(true);
    }

    private class TestFunction : Function
    {
        protected override IConfiguration BuildConfiguration(IHostEnvironment hostEnvironment)
        {
            var configuration = base.BuildConfiguration(hostEnvironment);
            return new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddInMemoryCollection(new KeyValuePair<string, string?>[]
                {
                    new("DistributedS3CacheOptions:Bucket", "road-registry-snapshots"),
                    new("DistributedS3CacheOptions:RootDir", "snapshots")
                })
                .Build();
        }
    }
}
