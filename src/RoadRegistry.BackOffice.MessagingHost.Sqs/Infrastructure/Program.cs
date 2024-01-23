namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Infrastructure;

using Abstractions.Configuration;
using Autofac;
using Configuration;
using Consumers;
using Extensions;
using Framework;
using Handlers.Sqs;
using Hosts;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

public class Program
{
    protected Program()
    {
    }
    
    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) => services
                .AddHostedService<FeatureCompareMessageConsumer>()
                .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
                .RegisterOptions<FeatureCompareMessagingOptions>()
                .AddRoadNetworkCommandQueue()
                .AddRoadNetworkEventWriter()
            )
            .ConfigureContainer((hostContext, builder) =>
            {
                builder.RegisterModule(new SqsHandlersModule());
                builder.RegisterModule(new MediatorModule());
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellKnownConnectionNames.Events
            })
            .Log((sp, logger) => {
                var blobClientOptions = sp.GetRequiredService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync();
    }
}
