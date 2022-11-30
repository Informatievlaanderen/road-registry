namespace RoadRegistry.Producer.Snapshot.ProjectionHost
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Hosts;
    using Hosts.Metadata;
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
    using Serilog.Debugging;
    using SqlStreamStore;
    using KafkaProducer = Projections.KafkaProducer;

    public class Program
    {
        protected Program()
        {
        }

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting RoadRegistry.Producer.Snapshot.ProjectionHost");

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.", AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var host = new HostBuilder()
                .ConfigureHostConfiguration(builder =>
                {
                    builder
                        .AddEnvironmentVariables("DOTNET_")
                        .AddEnvironmentVariables("ASPNETCORE_");
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    if (hostContext.HostingEnvironment.IsProduction())
                        builder
                            .SetBasePath(Directory.GetCurrentDirectory());

                    builder
                        .AddJsonFile("appsettings.json", true, false)
                        .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName.ToLowerInvariant()}.json", true, false)
                        .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                    SelfLog.Enable(Console.WriteLine);

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
                        .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetSection(MetadataConfiguration.Section).Get<MetadataConfiguration>())
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddSingleton<Scheduler>()
                        .AddHostedService<EventProcessor>()
                        .AddSingleton(new RecyclableMemoryStreamManager())
                        .AddSingleton(new EnvelopeFactory(
                            EventProcessor.EventMapping,
                            new EventDeserializer((eventData, eventType) =>
                                JsonConvert.DeserializeObject(eventData, eventType, EventProcessor.SerializerSettings)))
                        )
                        .AddSingleton(() =>
                                new ProducerSnapshotContext(
                                    new DbContextOptionsBuilder<ProducerSnapshotContext>()
                                        .UseSqlServer(
                                            hostContext.Configuration.GetConnectionString(WellknownConnectionNames.ProducerSnapshotProjections),
                                            options => options
                                                .EnableRetryOnFailure()
                                                .UseNetTopologySuite()
                                    ).Options)
                        )
                        .AddSingleton(sp =>
                        {
                            var config = sp.GetRequiredService<IConfiguration>();
                            var bootstrapServers = config["Kafka:BootstrapServers"];
                            var kafkaUserName = config["Kafka:SaslUserName"];
                            var kafkaPassword = config["Kafka:SaslPassword"];

                            return new ConnectedProjection<ProducerSnapshotContext>[]
                            {
                                new RoadNodeRecordProjection(new KafkaProducer(new KafkaProducerOptions(
                                    bootstrapServers,
                                    kafkaUserName,
                                    kafkaPassword,
                                    config["RoadNodeTopic"] ?? throw new ArgumentException($"Configuration has no value for RoadNodeTopic"),
                                    true,
                                    EventProcessor.SerializerSettings
                                )))
                            };
                        })
                        .AddSingleton(sp =>
                            Resolve
                                .WhenEqualToHandlerMessageType(
                                    sp.GetRequiredService<ConnectedProjection<ProducerSnapshotContext>[]>()
                                        .SelectMany(projection => projection.Handlers)
                                        .ToArray()
                                )
                        )
                        .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<ConnectedProjection<ProducerSnapshotContext>[]>(), EventProcessor.EventMapping))
                        .AddSingleton<IStreamStore>(sp =>
                            new MsSqlStreamStoreV3(
                                new MsSqlStreamStoreV3Settings(
                                    sp
                                        .GetService<IConfiguration>()
                                        .GetConnectionString(WellknownConnectionNames.Events)
                                )
                                { Schema = WellknownSchemas.EventSchema }))
                        .AddSingleton<IRunnerDbContextMigratorFactory>(new ProducerSnapshotContextMigrationFactory());
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
                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.ProducerSnapshotProjections);
                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.ProducerSnapshotProjectionsAdmin);

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
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
