namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests;

using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

public class FunctionTests
{
    [Fact]
    public async Task CanCreateFunction()
    {
        var function = new TestFunction();
        await function.Handler(new JObject(), new TestLambdaContext());

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
