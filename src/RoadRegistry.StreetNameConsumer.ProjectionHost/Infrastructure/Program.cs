namespace RoadRegistry.StreetNameConsumer.ProjectionHost.Infrastructure;

using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.Projector.Modules;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules;
using Schema;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) =>
            {
                var kafkaOptions = hostContext.Configuration.GetOptions<KafkaOptions>();
                if (string.IsNullOrEmpty(kafkaOptions.Consumers?.StreetName?.Topic))
                {
                    throw new ArgumentException("Configuration has no StreetName Consumer with a Topic.");
                }

                services
                    .AddSingleton<IRunnerDbContextMigratorFactory>(new StreetNameConsumerContextMigrationFactory())
                    .AddSingleton(kafkaOptions)
                    .AddHostedService<StreetNameConsumer>();
            })
            .ConfigureContainer((hostContext, builder) =>
                {
                    builder
                        .RegisterModule<ConsumerModule>()
                        .RegisterModule(new ApiModule(hostContext.Configuration))
                        .RegisterModule(new ProjectorModule(hostContext.Configuration));
                }
            )
            .Build();

        await roadRegistryHost
            .RunAsync(async (sp, host, configuration) =>
            {
                var migratorFactory = sp.GetRequiredService<IRunnerDbContextMigratorFactory>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                
                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            });
    }
}
