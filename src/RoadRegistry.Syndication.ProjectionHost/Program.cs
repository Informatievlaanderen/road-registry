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
using Projections.StreetNameEvents;
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
                .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetSection(StreetNameFeedConfiguration.Section).Get<StreetNameFeedConfiguration>())
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
                .AddHostedService(serviceProvider => new AtomFeedProcessor<StreetNameFeedConfiguration, StraatNaam>(
                    serviceProvider.GetRequiredService<IRegistryAtomFeedReader>(),
                    new AtomEnvelopeFactory(
                        EventSerializerMapping.CreateForNamespaceOf(typeof(StreetNameWasRegistered)),
                        new DataContractSerializer(typeof(SyndicationContent<StraatNaam>))),
                    serviceProvider.GetRequiredService<ConnectedProjectionHandlerResolver<SyndicationContext>>(),
                    serviceProvider.GetRequiredService<Func<SyndicationContext>>(),
                    serviceProvider.GetRequiredService<Scheduler>(),
                    serviceProvider.GetRequiredService<StreetNameFeedConfiguration>(),
                    serviceProvider.GetRequiredService<ILogger<AtomFeedProcessor<StreetNameFeedConfiguration, StraatNaam>>>()))
                .AddSingleton<AtomEnvelopeFactory>()
                .AddSingleton(
                    () =>
                        new SyndicationContext(
                            new DbContextOptionsBuilder<SyndicationContext>()
                                .UseSqlServer(
                                    hostContext.Configuration.GetConnectionString(WellknownConnectionNames.SyndicationProjections),
                                    options => options
                                        .EnableRetryOnFailure()
                                ).Options)
                )
                .AddSingleton(sp => new ConnectedProjection<SyndicationContext>[]
                {
                    new MunicipalityCacheProjection(),
                    new StreetNameCacheProjection()
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
                WellknownConnectionNames.Events,
                WellknownConnectionNames.SyndicationProjections,
                WellknownConnectionNames.SyndicationProjectionsAdmin
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                var migratorFactory = host.Services.GetRequiredService<IRunnerDbContextMigratorFactory>();
                var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
                var municipalityFeedConfiguration = host.Services.GetRequiredService<MunicipalityFeedConfiguration>();
                var streetNameFeedConfiguration = host.Services.GetRequiredService<StreetNameFeedConfiguration>();
                var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                var logger = host.Services.GetRequiredService<ILogger<Program>>();

                var municipalityToBecomeAvailable = WaitFor
                    .SyndicationApiToBecomeAvailable(httpClientFactory, municipalityFeedConfiguration, logger)
                    .ConfigureAwait(false);
                var streetNameToBecomeAvailable = WaitFor
                    .SyndicationApiToBecomeAvailable(httpClientFactory, streetNameFeedConfiguration, logger)
                    .ConfigureAwait(false);

                await municipalityToBecomeAvailable;
                await streetNameToBecomeAvailable;

                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            });
    }
}
