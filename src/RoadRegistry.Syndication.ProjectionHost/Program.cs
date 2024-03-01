namespace RoadRegistry.Syndication.ProjectionHost;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Projections;
using Projections.MunicipalityEvents;
using Projections.Syndication;
using Schema;
using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
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
                .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetSection(MunicipalityFeedConfiguration.Section).Get<MunicipalityFeedConfiguration>())
                .AddHttpClient(RegistryAtomFeedReader.HttpClientName)
                .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30)
                }))
                .Services
                .AddTransient<IRegistryAtomFeedReader, RegistryAtomFeedReader>()
                .AddHostedService(serviceProvider => new AtomFeedProcessor<MunicipalityFeedConfiguration, Gemeente>(
                    serviceProvider.GetRequiredService<IRegistryAtomFeedReader>(),
                    new AtomEnvelopeFactory(
                        EventSerializerMapping.CreateForNamespaceOf(typeof(MunicipalityWasRegistered)),
                        new DataContractSerializer(typeof(SyndicationContent<Gemeente>))),
                    serviceProvider.GetRequiredService<ConnectedProjectionHandlerResolver<SyndicationContext>>(),
                    serviceProvider.GetRequiredService<Func<SyndicationContext>>(),
                    serviceProvider.GetRequiredService<Scheduler>(),
                    serviceProvider.GetRequiredService<MunicipalityFeedConfiguration>(),
                    serviceProvider.GetRequiredService<ILogger<AtomFeedProcessor<MunicipalityFeedConfiguration, Gemeente>>>()))
                .AddSingleton<AtomEnvelopeFactory>()
                .AddSingleton(
                    () =>
                        new SyndicationContext(
                            new DbContextOptionsBuilder<SyndicationContext>()
                                .UseSqlServer(
                                    hostContext.Configuration.GetRequiredConnectionString(WellKnownConnectionNames.SyndicationProjections),
                                    options => options
                                        .EnableRetryOnFailure()
                                ).Options)
                )
                .AddSingleton(sp => new ConnectedProjection<SyndicationContext>[]
                {
                    new MunicipalityCacheProjection()
                })
                .AddSingleton(sp => Resolve.WhenEqualToHandlerMessageType(sp
                    .GetRequiredService<ConnectedProjection<SyndicationContext>[]>()
                    .SelectMany(projection => projection.Handlers)
                    .ToArray())
                )
                .AddSingleton<IRunnerDbContextMigratorFactory>(new SyndicationContextMigrationFactory()))
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.SyndicationProjections,
                WellKnownConnectionNames.SyndicationProjectionsAdmin
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                var migratorFactory = sp.GetRequiredService<IRunnerDbContextMigratorFactory>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = sp.GetRequiredService<ILogger<Program>>();

                var municipalityToBecomeAvailable = WaitFor
                    .SyndicationApiToBecomeAvailable(httpClientFactory, sp.GetRequiredService<MunicipalityFeedConfiguration>(), logger)
                    .ConfigureAwait(false);

                await municipalityToBecomeAvailable;

                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            });
    }
}
