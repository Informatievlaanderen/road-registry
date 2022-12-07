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
    using NationalRoad;
    using Newtonsoft.Json;
    using NodaTime;
    using RoadNode;
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
                    var runnerDbContextMigratorFactories = typeof(Program).Assembly
                        .GetTypes()
                        .Where(x => typeof(RunnerDbContextMigrationFactory<>).IsAssignableFrom(x))
                        .Select(type => (IRunnerDbContextMigratorFactory)Activator.CreateInstance(type))
                        .ToArray();

                    builder
                        .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetSection(MetadataConfiguration.Section).Get<MetadataConfiguration>())
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddTransient<Scheduler>()
                        .AddHostedService<RoadNodeEventProcessor>()
                        .AddSingleton(new RecyclableMemoryStreamManager())
                        .AddSingleton(new EnvelopeFactory(
                            RoadNodeEventProcessor.EventMapping,
                            new EventDeserializer((eventData, eventType) =>
                                JsonConvert.DeserializeObject(eventData, eventType, RoadNodeEventProcessor.SerializerSettings)))
                        )
                        .AddSingleton(() =>
                            new RoadNodeProducerSnapshotContext(
                                new DbContextOptionsBuilder<RoadNodeProducerSnapshotContext>()
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

                            return new ConnectedProjection<RoadNodeProducerSnapshotContext>[]
                            {
                                new RoadNodeRecordProjection(new KafkaProducer(new KafkaProducerOptions(
                                    config["Kafka:BootstrapServers"],
                                    config["Kafka:SaslUserName"],
                                    config["Kafka:SaslPassword"],
                                    config["RoadNodeTopic"] ?? throw new ArgumentException($"Configuration has no value for RoadNodeTopic"),
                                    true,
                                    RoadNodeEventProcessor.SerializerSettings
                                )))
                            };
                        })
                        .AddSingleton(sp =>
                            Resolve
                                .WhenEqualToHandlerMessageType(
                                    sp.GetRequiredService<ConnectedProjection<RoadNodeProducerSnapshotContext>[]>()
                                        .SelectMany(projection => projection.Handlers)
                                        .ToArray()
                                )
                        )
                        .AddSingleton(sp => RoadNodeAcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<ConnectedProjection<RoadNodeProducerSnapshotContext>[]>(), RoadNodeEventProcessor.EventMapping))
                        .AddTransient<IStreamStore>(sp =>
                            new MsSqlStreamStoreV3(
                                new MsSqlStreamStoreV3Settings(
                                        sp
                                            .GetService<IConfiguration>()
                                            .GetConnectionString(WellknownConnectionNames.Events)
                                    )
                                { Schema = WellknownSchemas.EventSchema }))

                        .AddSnapshotProducer<NationalRoadProducerSnapshotContext, NationalRoadRecordProjection, NationalRoadEventProcessor>(
                            "NationalRoad",
                            dbContextOptionsBuilder =>
                                new NationalRoadProducerSnapshotContext(dbContextOptionsBuilder.Options),
                            kafkaProducer =>
                                new NationalRoadRecordProjection(kafkaProducer),
                            connectedProjection =>
                                NationalRoadAcceptStreamMessage.WhenEqualToMessageType(connectedProjection, NationalRoadEventProcessor.EventMapping))

                        .AddSingleton(runnerDbContextMigratorFactories);
                    //.AddSingleton<IRunnerDbContextMigratorFactory[]>(
                    //    new IRunnerDbContextMigratorFactory[]
                    //    {
                    //        new RoadNodeProducerSnapshotContextMigrationFactory(),
                    //        new NationalRoadProducerSnapshotContextMigrationFactory()
                    //    });
                })
                .Build();

            var migratorFactories = host.Services.GetRequiredService<IRunnerDbContextMigratorFactory[]>();
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
                            foreach (var migratorFactory in migratorFactories)
                            {
                                await migratorFactory
                                    .CreateMigrator(configuration, loggerFactory)
                                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
                            }

                            Console.WriteLine("Started RoadRegistry.Producer.Snapshot.ProjectionHost");
                            await host.RunAsync().ConfigureAwait(false);
                        },
                        DistributedLockOptions.LoadFromConfiguration(configuration),
                        logger)
                    .ConfigureAwait(false);
            }
            catch (AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    logger.LogCritical(innerException, "Encountered a fatal exception, exiting program.");
                }
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

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSnapshotProducer<TSnapshotContext, TProjection, TEventProcessor>(this IServiceCollection services,
            string entityName,
            Func<DbContextOptionsBuilder<TSnapshotContext>, TSnapshotContext> resolveContext,
            Func<KafkaProducer, TProjection> resolveProjection,
            Func<ConnectedProjection<TSnapshotContext>[], AcceptStreamMessageFilter> buildAcceptStreamMessageFilter
        )
            where TSnapshotContext : RunnerDbContext<TSnapshotContext>
            where TEventProcessor : DbContextEventProcessor<TSnapshotContext>
            where TProjection : ConnectedProjection<TSnapshotContext>
        {
            return services
                .AddSingleton<Func<TSnapshotContext>>(sp =>
                {
                    return () =>
                    {
                        var configuration = sp.GetRequiredService<IConfiguration>();

                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TSnapshotContext>()
                            .UseSqlServer(
                                configuration.GetConnectionString(WellknownConnectionNames.ProducerSnapshotProjections),
                                options => options
                                    .EnableRetryOnFailure()
                                    .UseNetTopologySuite()
                            );

                        return resolveContext(dbContextOptionsBuilder);
                    };
                })
                .AddSingleton(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();

                    return new ConnectedProjection<TSnapshotContext>[]
                    {
                        resolveProjection(new KafkaProducer(new KafkaProducerOptions(
                            configuration["Kafka:BootstrapServers"],
                            configuration["Kafka:SaslUserName"],
                            configuration["Kafka:SaslPassword"],
                            configuration.GetRequiredValue<string>(entityName + "Topic"),
                            true,
                            RoadNodeEventProcessor.SerializerSettings
                        )))
                    };
                })
                .AddSingleton(sp =>
                    Resolve
                        .WhenEqualToHandlerMessageType(
                            sp.GetRequiredService<ConnectedProjection<TSnapshotContext>[]>()
                                .SelectMany(projection => projection.Handlers)
                                .ToArray()
                        )
                )
                .AddSingleton(sp => buildAcceptStreamMessageFilter(sp.GetRequiredService<ConnectedProjection<TSnapshotContext>[]>()))
                .AddHostedService<TEventProcessor>()
                ;
        }
    }
}
