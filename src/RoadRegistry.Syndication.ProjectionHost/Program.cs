namespace RoadRegistry.Syndication.ProjectionHost
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Mapping;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using NodaTime;
    using Polly;
    using Projections;
    using Projections.Syndication;
    using Schema;
    using Serilog;
    using SqlStreamStore;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting RoadRegistry.Syndication.ProjectionHost");

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.", AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var host = new HostBuilder()
                .ConfigureHostConfiguration(builder => {
                    builder
                        .AddEnvironmentVariables("DOTNET_")
                        .AddEnvironmentVariables("ASPNETCORE_");
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    if (hostContext.HostingEnvironment.IsProduction())
                    {
                        builder
                            .SetBasePath(Directory.GetCurrentDirectory());
                    }

                    builder
                        .AddJsonFile("appsettings.json", true, false)
                        .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName.ToLowerInvariant()}.json", true, false)
                        .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                    Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

                    var loggerConfiguration = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .Enrich.WithEnvironmentUserName();

                    Log.Logger = loggerConfiguration.CreateLogger();

                    builder.AddSerilog(Log.Logger);
                })
                .ConfigureServices((hostContext, builder) =>
                {
                    builder
                        .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetSection(MunicipalityFeedConfiguration.Section).Get<MunicipalityFeedConfiguration>())
                        .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetSection(StreetNameFeedConfiguration.Section).Get<StreetNameFeedConfiguration>())
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddSingleton<Scheduler>()
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
                            serviceProvider.GetRequiredService<AtomEnvelopeFactory>(),
                            serviceProvider.GetRequiredService<ConnectedProjectionHandlerResolver<SyndicationContext>>(),
                            serviceProvider.GetRequiredService<Func<SyndicationContext>>(),
                            serviceProvider.GetRequiredService<Scheduler>(),
                            serviceProvider.GetRequiredService<MunicipalityFeedConfiguration>(),
                            serviceProvider.GetRequiredService<ILogger<AtomFeedProcessor<MunicipalityFeedConfiguration, Gemeente>>>()))
                        .AddHostedService(serviceProvider => new AtomFeedProcessor<StreetNameFeedConfiguration, StraatNaam>(
                            serviceProvider.GetRequiredService<IRegistryAtomFeedReader>(),
                            serviceProvider.GetRequiredService<AtomEnvelopeFactory>(),
                            serviceProvider.GetRequiredService<ConnectedProjectionHandlerResolver<SyndicationContext>>(),
                            serviceProvider.GetRequiredService<Func<SyndicationContext>>(),
                            serviceProvider.GetRequiredService<Scheduler>(),
                            serviceProvider.GetRequiredService<StreetNameFeedConfiguration>(),
                            serviceProvider.GetRequiredService<ILogger<AtomFeedProcessor<StreetNameFeedConfiguration, StraatNaam>>>()))
                        .AddSingleton(new RecyclableMemoryStreamManager())
                        .AddSingleton<EventSerializerMapping>()
                        .AddSingleton<AtomEntrySerializerMapping>()
                        .AddSingleton<AtomEnvelopeFactory>()
                        .AddSingleton<Func<SyndicationContext>>(
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
                        .AddSingleton(sp =>
                            Resolve
                                .WhenEqualToHandlerMessageType(
                            sp.GetRequiredService<ConnectedProjection<SyndicationContext>[]>()
                                    .SelectMany(projection => projection.Handlers)
                                    .ToArray()
                                )
                        )
                        .AddSingleton<IStreamStore>(sp =>
                            new MsSqlStreamStoreV3(
                                new MsSqlStreamStoreV3Settings(
                                    sp
                                        .GetService<IConfiguration>()
                                        .GetConnectionString(WellknownConnectionNames.Events)
                                ) {Schema = WellknownSchemas.EventSchema}))
                        .AddSingleton<IRunnerDbContextMigratorFactory>(new SyndicationContextMigrationFactory());
                })
                .Build();

            var migratorFactory = host.Services.GetRequiredService<IRunnerDbContextMigratorFactory>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
            var municipalityFeedConfiguration = host.Services.GetRequiredService<MunicipalityFeedConfiguration>();
            var streetNameFeedConfiguration = host.Services.GetRequiredService<StreetNameFeedConfiguration>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            try
            {
                await WaitFor.SeqToBecomeAvailable(configuration).ConfigureAwait(false);

                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Events);
                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.SyndicationProjections);
                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.SyndicationProjectionsAdmin);

                await DistributedLock<Program>.RunAsync(async () =>
                    {
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

                        await host.RunAsync().ConfigureAwait(false);
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration),
                    logger)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
