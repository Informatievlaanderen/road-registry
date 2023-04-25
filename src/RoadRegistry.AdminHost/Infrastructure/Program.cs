namespace RoadRegistry.AdminHost.Infrastructure;

using Autofac;
using BackOffice;
using BackOffice.Configuration;
using BackOffice.Extensions;
using BackOffice.Framework;
using BackOffice.Handlers.Sqs;
using Consumers;
using Hosts;
using Hosts.Infrastructure.Extensions;
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
                .AddSingleton<AdminMessageConsumer>()
                .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
                .AddScoped(_ => new EventSourcedEntityMap())
                .AddRoadNetworkCommandQueue()
                .AddRoadNetworkEventWriter()
                .AddRoadRegistrySnapshot()
                .AddRoadNetworkSnapshotStrategyOptions()
                .AddEditorContext()
            )
            .ConfigureContainer((hostContext, builder) =>
            {
                builder.RegisterModule<MediatorModule>();
                builder.RegisterModule<Snapshot.Handlers.MediatorModule>();

                builder.RegisterModule<ContextModule>();
                builder.RegisterModule<SqsHandlersModule>();
            })
            .ConfigureRunCommand(async (sp, stoppingToken) =>
            {
                var service = sp.GetRequiredService<AdminMessageConsumer>();
                await service.ExecuteAsync(stoppingToken);
            })
            .Build();

        await roadRegistryHost
            .Log((sp, logger) => {
                logger.LogKnownSqlServerConnectionStrings(roadRegistryHost.Configuration);

                var blobClientOptions = sp.GetRequiredService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync();
    }
}
