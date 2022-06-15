namespace RoadRegistry.BackOffice.CommandHost
{
    using System;
    using Microsoft.Data.SqlClient;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
    using Be.Vlaanderen.Basisregisters.BlobStore.IO;
    using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
    using Configuration;
    using Core;
    using Extracts;
    using Framework;
    using Hosts;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IO;
    using NodaTime;
    using Serilog;
    using SqlStreamStore;
    using Uploads;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting RoadRegistry.BackOffice.CommandHost");

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
                    var blobOptions = new BlobClientOptions();
                    hostContext.Configuration.Bind(blobOptions);

                    switch (blobOptions.BlobClientType)
                    {
                        case nameof(S3BlobClient):
                            var s3Options = new S3BlobClientOptions();
                            hostContext.Configuration.GetSection(nameof(S3BlobClientOptions)).Bind(s3Options);

                            // Use MINIO
                            if (hostContext.Configuration.GetValue<string>("MINIO_SERVER") != null)
                            {
                                if (hostContext.Configuration.GetValue<string>("MINIO_ACCESS_KEY") == null)
                                {
                                    throw new InvalidOperationException("The MINIO_ACCESS_KEY configuration variable was not set.");
                                }

                                if (hostContext.Configuration.GetValue<string>("MINIO_SECRET_KEY") == null)
                                {
                                    throw new InvalidOperationException("The MINIO_SECRET_KEY configuration variable was not set.");
                                }

                                builder.AddSingleton(new AmazonS3Client(
                                        new BasicAWSCredentials(
                                            hostContext.Configuration.GetValue<string>("MINIO_ACCESS_KEY"),
                                            hostContext.Configuration.GetValue<string>("MINIO_SECRET_KEY")),
                                        new AmazonS3Config
                                        {
                                            RegionEndpoint = RegionEndpoint.USEast1, // minio's default region
                                            ServiceURL = hostContext.Configuration.GetValue<string>("MINIO_SERVER"),
                                            ForcePathStyle = true
                                        }
                                    )
                                );

                            }
                            else // Use AWS
                            {
                                if (hostContext.Configuration.GetValue<string>("AWS_ACCESS_KEY_ID") == null)
                                {
                                    throw new InvalidOperationException("The AWS_ACCESS_KEY_ID configuration variable was not set.");
                                }

                                if (hostContext.Configuration.GetValue<string>("AWS_SECRET_ACCESS_KEY") == null)
                                {
                                    throw new InvalidOperationException("The AWS_SECRET_ACCESS_KEY configuration variable was not set.");
                                }

                                builder.AddSingleton(new AmazonS3Client(
                                        new BasicAWSCredentials(
                                            hostContext.Configuration.GetValue<string>("AWS_ACCESS_KEY_ID"),
                                            hostContext.Configuration.GetValue<string>("AWS_SECRET_ACCESS_KEY"))
                                    )
                                );
                            }

                            builder
                                .AddSingleton(sp =>
                                    new RoadNetworkUploadsBlobClient(new S3BlobClient(
                                        sp.GetRequiredService<AmazonS3Client>(),
                                        s3Options.Buckets[WellknownBuckets.UploadsBucket]
                                    )))
                                .AddSingleton(sp =>
                                    new RoadNetworkExtractUploadsBlobClient(new S3BlobClient(
                                        sp.GetRequiredService<AmazonS3Client>(),
                                        s3Options.Buckets[WellknownBuckets.UploadsBucket]
                                    )));

                            break;

                        case nameof(FileBlobClient):
                            var fileOptions = new FileBlobClientOptions();
                            hostContext.Configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);

                            builder
                                .AddSingleton<IBlobClient>(sp =>
                                    new FileBlobClient(
                                        new DirectoryInfo(fileOptions.Directory)
                                    )
                                )
                                .AddSingleton<RoadNetworkUploadsBlobClient>()
                                .AddSingleton<RoadNetworkExtractUploadsBlobClient>();
                            break;

                        default:
                            throw new InvalidOperationException(blobOptions.BlobClientType + " is not a supported blob client type.");
                    }

                    builder
                        .AddSingleton<Scheduler>()
                        .AddHostedService<RoadNetworkCommandProcessor>()
                        .AddHostedService<RoadNetworkExtractCommandProcessor>()
                        .AddSingleton<ICommandProcessorPositionStore>(sp =>
                            new SqlCommandProcessorPositionStore(
                                new SqlConnectionStringBuilder(
                                    sp.GetService<IConfiguration>().GetConnectionString(WellknownConnectionNames.CommandHost)
                                ),
                                WellknownSchemas.CommandHostSchema))
                        .AddSingleton<IStreamStore>(sp =>
                            new MsSqlStreamStoreV3(
                                new MsSqlStreamStoreV3Settings(
                                    sp
                                        .GetService<IConfiguration>()
                                        .GetConnectionString(WellknownConnectionNames.Events))
                                {
                                    Schema = WellknownSchemas.EventSchema
                                }))
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddSingleton(new RecyclableMemoryStreamManager())
                        .AddSingleton(sp => new RoadNetworkSnapshotReaderWriter(
                            new RoadNetworkSnapshotsBlobClient(
                                new SqlBlobClient(
                                    new SqlConnectionStringBuilder(
                                        sp
                                            .GetService<IConfiguration>()
                                            .GetConnectionString(WellknownConnectionNames.Snapshots)),
                                    WellknownSchemas.SnapshotSchema)),
                            sp.GetService<RecyclableMemoryStreamManager>()))
                        .AddSingleton<IRoadNetworkSnapshotReader>(sp => sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
                        .AddSingleton<IRoadNetworkSnapshotWriter>(sp => sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
                        .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                            new CommandHandlerModule[]
                            {
                                new RoadNetworkChangesArchiveCommandModule(
                                    sp.GetService<RoadNetworkUploadsBlobClient>(),
                                    sp.GetService<IStreamStore>(),
                                    sp.GetService<IRoadNetworkSnapshotReader>(),
                                    new ZipArchiveValidator(Encoding.GetEncoding(1252)),
                                    sp.GetService<IClock>()
                                ),
                                new RoadNetworkCommandModule(
                                    sp.GetService<IStreamStore>(),
                                    sp.GetService<IRoadNetworkSnapshotReader>(),
                                    sp.GetService<IRoadNetworkSnapshotWriter>(),
                                    sp.GetService<IClock>()
                                ),
                                new RoadNetworkExtractCommandModule(
                                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                                    sp.GetService<IStreamStore>(),
                                    sp.GetService<IRoadNetworkSnapshotReader>(),
                                    new ZipArchiveValidator(Encoding.GetEncoding(1252)),
                                    sp.GetService<IClock>()
                                )
                            })));
                })
                .Build();

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var streamStore = host.Services.GetRequiredService<IStreamStore>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var blobClientOptions = new BlobClientOptions();
            configuration.Bind(blobClientOptions);

            try
            {
                await WaitFor.SeqToBecomeAvailable(configuration).ConfigureAwait(false);

                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Events);
                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.CommandHost);
                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.CommandHostAdmin);
                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Snapshots);
                logger.LogBlobClientCredentials(blobClientOptions);

                await DistributedLock<Program>.RunAsync(async () =>
                    {
                        await WaitFor.SqlStreamStoreToBecomeAvailable(streamStore, logger).ConfigureAwait(false);
                        await
                            new SqlCommandProcessorPositionStoreSchema(
                                new SqlConnectionStringBuilder(
                                    configuration.GetConnectionString(WellknownConnectionNames.CommandHostAdmin))
                            ).CreateSchemaIfNotExists(WellknownSchemas.CommandHostSchema).ConfigureAwait(false);
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
