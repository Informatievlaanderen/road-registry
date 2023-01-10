namespace RoadRegistry.BackOffice.ExtractHost;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abstractions;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Core;
using Editor.Schema;
using Extracts;
using Framework;
using Hosts;
using Hosts.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NodaTime;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using Serilog;
using Serilog.Debugging;
using SqlStreamStore;
using Syndication.Schema;
using Uploads;
using ZipArchiveWriters.ExtractHost;

public class Program
{
    private static readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.BackOffice);

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting {0}", typeof(Program).Namespace);

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
                            if (hostContext.Configuration.GetValue<string>("MINIO_ACCESS_KEY") == null) throw new InvalidOperationException("The MINIO_ACCESS_KEY configuration variable was not set.");

                            if (hostContext.Configuration.GetValue<string>("MINIO_SECRET_KEY") == null) throw new InvalidOperationException("The MINIO_SECRET_KEY configuration variable was not set.");

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
                            builder.AddSingleton(new AmazonS3Client());
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
                                )))
                            .AddSingleton(sp =>
                                new RoadNetworkExtractDownloadsBlobClient(new S3BlobClient(
                                    sp.GetRequiredService<AmazonS3Client>(),
                                    s3Options.Buckets[WellknownBuckets.ExtractDownloadsBucket]
                                )))
                            .AddSingleton(sp =>
                                new RoadNetworkFeatureCompareBlobClient(new S3BlobClient(
                                    sp.GetRequiredService<AmazonS3Client>(),
                                    s3Options.Buckets[WellknownBuckets.FeatureCompareBucket]
                                )))
                            ;

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
                            .AddSingleton<RoadNetworkExtractUploadsBlobClient>()
                            .AddSingleton<RoadNetworkExtractDownloadsBlobClient>()
                            .AddSingleton<RoadNetworkFeatureCompareBlobClient>();
                        break;

                    default:
                        throw new InvalidOperationException(blobOptions.BlobClientType + " is not a supported blob client type.");
                }

                var zipArchiveWriterOptions = new ZipArchiveWriterOptions();
                hostContext.Configuration.GetSection(nameof(ZipArchiveWriterOptions)).Bind(zipArchiveWriterOptions);

                builder
                    .AddSingleton<Scheduler>()
                    .AddHostedService<EventProcessor>()
                    .AddSingleton<IEventProcessorPositionStore>(sp =>
                        new SqlEventProcessorPositionStore(
                            new SqlConnectionStringBuilder(
                                sp.GetService<IConfiguration>().GetConnectionString(WellknownConnectionNames.ExtractHost)
                            ),
                            WellknownSchemas.ExtractHostSchema))
                    .AddStreamStore()
                    .AddSingleton<IClock>(SystemClock.Instance)
                    .AddSingleton(new RecyclableMemoryStreamManager())
                    .AddSingleton(sp => new RoadNetworkSnapshotReaderWriter(
                        new RoadNetworkSnapshotsBlobClient(
                            new SqlBlobClient(
                                new SqlConnectionStringBuilder(
                                    sp
                                        .GetService<IConfiguration>()
                                        .GetConnectionString(WellknownConnectionNames.Snapshots)
                                ), WellknownSchemas.SnapshotSchema)),
                        sp.GetService<RecyclableMemoryStreamManager>()))
                    .AddSingleton<IStreetNameCache, StreetNameCache>()
                    .AddSingleton<Func<SyndicationContext>>(sp =>
                        () =>
                            new SyndicationContext(
                                new DbContextOptionsBuilder<SyndicationContext>()
                                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                    .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                                    .UseSqlServer(
                                        hostContext.Configuration.GetConnectionString(WellknownConnectionNames.SyndicationProjections),
                                        options => options
                                            .EnableRetryOnFailure()
                                    ).Options)
                    )
                    .AddSingleton(zipArchiveWriterOptions)
                    .AddSingleton<IZipArchiveWriter<EditorContext>>(sp =>
                        new RoadNetworkExtractToZipArchiveWriter(
                            sp.GetService<ZipArchiveWriterOptions>(),
                            sp.GetService<IStreetNameCache>(),
                            sp.GetService<RecyclableMemoryStreamManager>(),
                            Encoding.GetEncoding(1252)))
                    .AddSingleton<Func<EditorContext>>(sp =>
                        () =>
                            new EditorContext(
                                new DbContextOptionsBuilder<EditorContext>()
                                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                    .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                                    .UseSqlServer(
                                        hostContext.Configuration.GetConnectionString(WellknownConnectionNames.EditorProjections),
                                        options => options
                                            .UseNetTopologySuite()
                                    ).Options)
                    )
                    .AddSingleton<IRoadNetworkExtractArchiveAssembler>(sp =>
                        new RoadNetworkExtractArchiveAssembler(
                            sp.GetService<RecyclableMemoryStreamManager>(),
                            sp.GetService<Func<EditorContext>>(),
                            sp.GetService<IZipArchiveWriter<EditorContext>>()))
                    .AddSingleton(sp => new EventHandlerModule[]
                    {
                        new RoadNetworkExtractEventModule(
                            sp.GetService<RoadNetworkExtractDownloadsBlobClient>(),
                            sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                            sp.GetService<IRoadNetworkExtractArchiveAssembler>(),
                            new ZipArchiveTranslator(Encoding.GetEncoding(1252)),
                            sp.GetService<IStreamStore>(),
                            ApplicationMetadata)
                    })
                    .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<EventHandlerModule[]>(), EventProcessor.EventMapping))
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(sp.GetRequiredService<EventHandlerModule[]>())));
            })
            .Build();

        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var streamStore = host.Services.GetRequiredService<IStreamStore>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var blobClient = host.Services.GetRequiredService<RoadNetworkExtractDownloadsBlobClient>();
        var blobClientOptions = new BlobClientOptions();
        configuration.Bind(blobClientOptions);

        try
        {
            await WaitFor.SeqToBecomeAvailable(configuration).ConfigureAwait(false);

            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Events);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.ExtractHost);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.ExtractHostAdmin);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Snapshots);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.SnapshotsAdmin);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.EditorProjections);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.SyndicationProjections);
            logger.LogBlobClientCredentials(blobClientOptions);

            await DistributedLock<Program>.RunAsync(async () =>
                    {
                        await WaitFor.SqlStreamStoreToBecomeAvailable(streamStore, logger).ConfigureAwait(false);
                        await
                            new SqlBlobSchema(
                                new SqlConnectionStringBuilder(configuration.GetConnectionString(WellknownConnectionNames.SnapshotsAdmin))
                            ).CreateSchemaIfNotExists(WellknownSchemas.SnapshotSchema).ConfigureAwait(false);
                        await
                            new SqlEventProcessorPositionStoreSchema(
                                new SqlConnectionStringBuilder(configuration.GetConnectionString(WellknownConnectionNames.ExtractHostAdmin))
                            ).CreateSchemaIfNotExists(WellknownSchemas.ExtractHostSchema).ConfigureAwait(false);
                        await blobClient.ProvisionResources(host).ConfigureAwait(false);

                        Console.WriteLine("Started {0}", typeof(Program).Namespace);
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
