namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Infrastructure;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Configuration;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Consumers;
using Core;
using Extracts;
using Framework;
using Handlers.Sqs;
using Hosts;
using Hosts.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NodaTime;
using Serilog;
using Serilog.Debugging;
using SqlStreamStore;
using Uploads;
using ZipArchiveWriters.Validation;

public class Program
{
    private static IServiceCollection _serviceCollection = new ServiceCollection();

    protected Program()
    {
    }

    private static void AddBlobClients(IServiceCollection services, IConfiguration configuration)
    {
        var blobOptions = new BlobClientOptions();
        configuration.Bind(blobOptions);

        switch (blobOptions.BlobClientType)
        {
            case nameof(S3BlobClient):
                var s3Options = new S3BlobClientOptions();
                configuration.GetSection(nameof(S3BlobClientOptions)).Bind(s3Options);

                var s3BlobClient = GetAmazonS3Client();

                services
                    .AddSingleton(s3BlobClient)
                    .AddSingleton(sp => new RoadNetworkUploadsBlobClient(new S3BlobClient(sp.GetRequiredService<AmazonS3Client>(), s3Options.Buckets[WellknownBuckets.UploadsBucket])))
                    .AddSingleton(sp => new RoadNetworkExtractUploadsBlobClient(new S3BlobClient(sp.GetRequiredService<AmazonS3Client>(), s3Options.Buckets[WellknownBuckets.UploadsBucket])))
                    .AddSingleton(sp => new RoadNetworkExtractDownloadsBlobClient(new S3BlobClient(sp.GetRequiredService<AmazonS3Client>(), s3Options.Buckets[WellknownBuckets.ExtractDownloadsBucket])))
                    .AddSingleton(sp => new RoadNetworkFeatureCompareBlobClient(new S3BlobClient(sp.GetRequiredService<AmazonS3Client>(), s3Options.Buckets[WellknownBuckets.FeatureCompareBucket])));
                break;

            case nameof(FileBlobClient):
                var fileBlobClient = GetFileBlobClient();

                services
                    .AddSingleton(fileBlobClient)
                    .AddSingleton<RoadNetworkUploadsBlobClient>()
                    .AddSingleton<RoadNetworkExtractUploadsBlobClient>()
                    .AddSingleton<RoadNetworkExtractDownloadsBlobClient>()
                    .AddSingleton<RoadNetworkFeatureCompareBlobClient>();
                break;

            default: throw new InvalidOperationException(blobOptions.BlobClientType + " is not a supported blob client type.");
        }

        AmazonS3Client GetAmazonS3Client()
        {
            if (configuration.GetValue<string>("MINIO_SERVER") != null)
            {
                var (accessKey, secretKey) = GetAccessKey("MINIO_ACCESS_KEY", "MINIO_SECRET_KEY");

                return new AmazonS3Client(
                    new BasicAWSCredentials(accessKey, secretKey),
                    new AmazonS3Config
                    {
                        RegionEndpoint = RegionEndpoint.USEast1, // minio's default region
                        ServiceURL = configuration.GetValue<string>("MINIO_SERVER"),
                        ForcePathStyle = true
                    }
                );
            }
            else
            {
                return new AmazonS3Client();
            }
        }

        IBlobClient GetFileBlobClient()
        {
            var fileOptions = new FileBlobClientOptions();
            configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);
            return new FileBlobClient(new DirectoryInfo(fileOptions.Directory));
        }

        (string, string) GetAccessKey(string keyId, string keySecret)
        {
            var accessKey = configuration.GetValue<string>(keyId);
            var secretKey = configuration.GetValue<string>(keySecret);

            ArgumentNullException.ThrowIfNull(accessKey);
            ArgumentNullException.ThrowIfNull(secretKey);

            return (accessKey, secretKey);
        }
    }

    private static void ConfigureContainer(ContainerBuilder builder)
    {
        builder.Populate(_serviceCollection);
        builder.RegisterModule(new MediatorModule());
        builder.RegisterModule(new Handlers.Sqs.MediatorModule());
        builder.RegisterModule(new SqsHandlersModule());
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting RoadRegistry.BackOffice.MessagingHost.Sqs");

        AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.", AppDomain.CurrentDomain.FriendlyName);

        AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

        var host = new HostBuilder()
            .ConfigureHostConfiguration(services =>
            {
                services
                    .AddEnvironmentVariables("DOTNET_")
                    .AddEnvironmentVariables("ASPNETCORE_");
            })
            .ConfigureAppConfiguration((hostContext, services) =>
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                if (hostContext.HostingEnvironment.IsProduction())
                {
                    services
                        .SetBasePath(Directory.GetCurrentDirectory());
                }

                services
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
            .ConfigureServices((hostContext, services) =>
            {
                AddBlobClients(services, hostContext.Configuration);

                var featureCompareMessagingOptions = new FeatureCompareMessagingOptions();
                hostContext.Configuration.GetSection(FeatureCompareMessagingOptions.ConfigurationKey).Bind(featureCompareMessagingOptions);

                services
                    .AddHostedService<FeatureCompareMessageConsumer>()
                    .AddSingleton<Scheduler>()
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
                    .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => new EventSourcedEntityMap())
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                        new CommandHandlerModule[]
                        {
                            new RoadNetworkChangesArchiveCommandModule(
                                sp.GetService<RoadNetworkUploadsBlobClient>(),
                                sp.GetService<IStreamStore>(),
                                sp.GetService<Func<EventSourcedEntityMap>>(),
                                sp.GetService<IRoadNetworkSnapshotReader>(),
                                new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                                sp.GetService<IClock>(),
                                sp.GetService<ILogger<RoadNetworkChangesArchiveCommandModule>>()

                            ),
                            new RoadNetworkCommandModule(
                                sp.GetService<IStreamStore>(),
                                sp.GetService<Func<EventSourcedEntityMap>>(),
                                sp.GetService<IRoadNetworkSnapshotReader>(),
                                sp.GetService<IRoadNetworkSnapshotWriter>(),
                                sp.GetService<IClock>(),
                                sp.GetService<ILogger<RoadNetworkCommandModule>>()
                            ),
                            new RoadNetworkExtractCommandModule(
                                sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                                sp.GetService<IStreamStore>(),
                                sp.GetService<Func<EventSourcedEntityMap>>(),
                                sp.GetService<IRoadNetworkSnapshotReader>(),
                                new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                                sp.GetService<IClock>(),
                                sp.GetService<ILogger<RoadNetworkExtractCommandModule>>()
                            )
                        })))
                    .AddSingleton(featureCompareMessagingOptions);
                _serviceCollection = services;
            })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
            .Build();

        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var streamStore = host.Services.GetRequiredService<IStreamStore>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        var blobClientOptions = new BlobClientOptions();
        configuration.Bind(blobClientOptions);

        var messagingOptions = new FeatureCompareMessagingOptions();
        configuration.GetSection(FeatureCompareMessagingOptions.ConfigurationKey).Bind(messagingOptions);

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

                        Console.WriteLine("Started RoadRegistry.BackOffice.MessagingHost.Sqs");
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
            await Log.CloseAndFlushAsync().ConfigureAwait(false);
        }
    }
}
