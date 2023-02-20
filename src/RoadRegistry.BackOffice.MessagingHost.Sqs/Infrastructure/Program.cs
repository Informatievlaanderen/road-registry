namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Infrastructure;

using Abstractions;
using Abstractions.Configuration;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
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
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Uploads;
using ZipArchiveWriters.Validation;

public class Program
{
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

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureOptions<FeatureCompareMessagingOptions>(FeatureCompareMessagingOptions.ConfigurationKey, out var featureCompareMessagingOptions)
            .ConfigureServices((hostContext, services) => services
                .AddHostedService<FeatureCompareMessageConsumer>()
                .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => new EventSourcedEntityMap()))
            .ConfigureCommandDispatcher(sp => Resolve.WhenEqualToMessage(new CommandHandlerModule[] {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetService<RoadNetworkUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()

                ),
                new RoadNetworkCommandModule(
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                )
            }))
            .ConfigureContainer((hostContext, builder) =>
            {
                builder.RegisterModule(new MediatorModule());
                builder.RegisterModule(new SqsHandlersModule());
                builder.RegisterModule(new Handlers.Sqs.MediatorModule());
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.CommandHost,
                WellknownConnectionNames.CommandHostAdmin,
                WellknownConnectionNames.Snapshots
            })
            .Log((sp, logger) => {
                var blobClientOptions = sp.GetService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                await
                    new SqlCommandProcessorPositionStoreSchema(
                        new SqlConnectionStringBuilder(
                            configuration.GetConnectionString(WellknownConnectionNames.CommandHostAdmin))
                    ).CreateSchemaIfNotExists(WellknownSchemas.CommandHostSchema).ConfigureAwait(false);
            });
    }
}
