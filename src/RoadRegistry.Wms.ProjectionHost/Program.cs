namespace RoadRegistry.Wms.ProjectionHost
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
    using Be.Vlaanderen.Basisregisters.BlobStore.IO;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Configuration;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using Newtonsoft.Json;
    using NodaTime;
    using Projections;
    using Schema;
    using Serilog;
    using SqlStreamStore;

    public class Program
    {
        private static readonly Encoding WindowsAnsiEncoding = Encoding.GetEncoding(1252);

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting RoadRegistry.Wms.ProjectionHost");

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
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddSingleton<Scheduler>()
                        .AddHostedService<EventProcessor>()
                        .AddSingleton(new RecyclableMemoryStreamManager())
                        .AddSingleton(new EnvelopeFactory(
                            EventProcessor.EventMapping,
                            new EventDeserializer((eventData, eventType) =>
                                JsonConvert.DeserializeObject(eventData, eventType, EventProcessor.SerializerSettings)))
                        )
                        .AddSingleton<Func<WmsContext>>(
                            () =>
                                new WmsContext(
                                    new DbContextOptionsBuilder<WmsContext>()
                                        .UseSqlServer(
                                            hostContext.Configuration.GetConnectionString(WellknownConnectionNames.WmsProjections),
                                            options => options.EnableRetryOnFailure()
                                        ).Options)
                        )
                        .AddSingleton(sp => new ConnectedProjection<WmsContext>[]
                        {
                            new RoadSegmentRecordProjection(WindowsAnsiEncoding)
                        })
                        .AddSingleton(sp =>
                            Resolve
                                .WhenEqualToHandlerMessageType(
                            sp.GetRequiredService<ConnectedProjection<WmsContext>[]>()
                                    .SelectMany(projection => projection.Handlers)
                                    .ToArray()
                                )
                        )
                        .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<ConnectedProjection<WmsContext>[]>(), EventProcessor.EventMapping))
                        .AddSingleton<IStreamStore>(sp =>
                            new MsSqlStreamStore(
                                new MsSqlStreamStoreSettings(
                                    sp
                                        .GetService<IConfiguration>()
                                        .GetConnectionString(WellknownConnectionNames.Events)
                                ) {Schema = WellknownSchemas.EventSchema}))
                        .AddSingleton<IRunnerDbContextMigratorFactory>(new WmsContextMigrationFactory());
                })
                .Build();

            var migratorFactory = host.Services.GetRequiredService<IRunnerDbContextMigratorFactory>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var streamStore = host.Services.GetRequiredService<IStreamStore>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            try
            {
                await WaitFor.SeqToBecomeAvailable(configuration).ConfigureAwait(false);

                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Events);
                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.WmsProjections);
                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.WmsProjectionsAdmin);

                await DistributedLock<Program>.RunAsync(async () =>
                    {
                        await WaitFor.SqlStreamStoreToBecomeAvailable(streamStore, logger).ConfigureAwait(false);
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
