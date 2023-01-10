namespace RoadRegistry.BackOffice.Api;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Configuration;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Core;
using Editor.Schema;
using Hosts;
using Hosts.Configuration;
using Hosts.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using Product.Schema;
using Serilog;
using SqlStreamStore;
using Syndication.Schema;
using ZipArchiveWriters.Validation;

public class Program
{
    public const int HostingPort = 10002;

    protected Program()
    {
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        var webHostBuilder = new WebHostBuilder()
            .UseDefaultForApi<Startup>(
                new ProgramOptions
                {
                    Hosting =
                    {
                        HttpPort = HostingPort
                    },
                    Logging =
                    {
                        WriteTextToConsole = false,
                        WriteJsonToConsole = false
                    },
                    Runtime =
                    {
                        CommandLineArgs = args
                    }
                })
            .UseKestrel((context, builder) =>
            {
                if (context.HostingEnvironment.EnvironmentName == "Development")
                {
                    builder.ListenLocalhost(HostingPort);
                }
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

                        var s3BlobClient = GetAmazonS3Client();

                        builder
                            .AddSingleton(s3BlobClient)
                            .AddSingleton(sp => new RoadNetworkUploadsBlobClient(new S3BlobClient(sp.GetRequiredService<AmazonS3Client>(), s3Options.Buckets[WellknownBuckets.UploadsBucket])))
                            .AddSingleton(sp => new RoadNetworkExtractUploadsBlobClient(new S3BlobClient(sp.GetRequiredService<AmazonS3Client>(), s3Options.Buckets[WellknownBuckets.UploadsBucket])))
                            .AddSingleton(sp => new RoadNetworkExtractDownloadsBlobClient(new S3BlobClient(sp.GetRequiredService<AmazonS3Client>(), s3Options.Buckets[WellknownBuckets.ExtractDownloadsBucket])))
                            .AddSingleton(sp => new RoadNetworkFeatureCompareBlobClient(new S3BlobClient(sp.GetRequiredService<AmazonS3Client>(), s3Options.Buckets[WellknownBuckets.FeatureCompareBucket])));
                        break;

                    case nameof(FileBlobClient):
                        var fileBlobClient = GetFileBlobClient();

                        builder
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
                    if (hostContext.Configuration.GetValue<string>("MINIO_SERVER") != null)
                    {
                        var (accessKey, secretKey) = GetAccessKey("MINIO_ACCESS_KEY", "MINIO_SECRET_KEY");

                        return new AmazonS3Client(
                            new BasicAWSCredentials(accessKey, secretKey),
                            new AmazonS3Config
                            {
                                RegionEndpoint = RegionEndpoint.USEast1, // minio's default region
                                ServiceURL = hostContext.Configuration.GetValue<string>("MINIO_SERVER"),
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
                    hostContext.Configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);
                    return new FileBlobClient(new DirectoryInfo(fileOptions.Directory));
                }

                (string, string) GetAccessKey(string keyId, string keySecret)
                {
                    var accessKey = hostContext.Configuration.GetValue<string>(keyId);
                    var secretKey = hostContext.Configuration.GetValue<string>(keySecret);

                    ArgumentNullException.ThrowIfNull(accessKey);
                    ArgumentNullException.ThrowIfNull(secretKey);

                    return (accessKey, secretKey);
                }

                var zipArchiveWriterOptions = new ZipArchiveWriterOptions();
                hostContext.Configuration.GetSection(nameof(ZipArchiveWriterOptions)).Bind(zipArchiveWriterOptions);
                var extractDownloadsOptions = new ExtractDownloadsOptions();
                hostContext.Configuration.GetSection(nameof(ExtractDownloadsOptions)).Bind(extractDownloadsOptions);
                var extractUploadsOptions = new ExtractUploadsOptions();
                hostContext.Configuration.GetSection(nameof(ExtractUploadsOptions)).Bind(extractDownloadsOptions);

                var featureCompareMessagingOptions = new FeatureCompareMessagingOptions();
                hostContext.Configuration.GetSection(FeatureCompareMessagingOptions.ConfigurationKey).Bind(featureCompareMessagingOptions);

                builder
                    .AddSingleton(new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
                    .AddSingleton<IZipArchiveBeforeFeatureCompareValidator>(new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8))
                    .AddSingleton<IZipArchiveAfterFeatureCompareValidator>(new ZipArchiveAfterFeatureCompareValidator(Encoding.UTF8))
                    .AddSingleton<ProblemDetailsHelper>()
                    .AddSingleton(zipArchiveWriterOptions)
                    .AddSingleton(extractDownloadsOptions)
                    .AddSingleton(extractUploadsOptions)
                    .AddSingleton(featureCompareMessagingOptions)
                    .AddStreamStore()
                    .AddSingleton<IClock>(SystemClock.Instance)
                    .AddSingleton(new WKTReader(
                        new NtsGeometryServices(
                            GeometryConfiguration.GeometryFactory.PrecisionModel,
                            GeometryConfiguration.GeometryFactory.SRID
                        )
                    ))
                    .AddSingleton(new RecyclableMemoryStreamManager())
                    .AddSingleton<IBlobClient>(new SqlBlobClient(
                        new SqlConnectionStringBuilder(
                            hostContext.Configuration.GetConnectionString(WellknownConnectionNames.Snapshots)),
                        WellknownSchemas.SnapshotSchema))
                    .AddSingleton(sp => new RoadNetworkSnapshotReaderWriter(
                        new RoadNetworkSnapshotsBlobClient(sp.GetService<IBlobClient>()),
                        sp.GetService<RecyclableMemoryStreamManager>()))
                    .AddSingleton<IRoadNetworkSnapshotReader>(sp =>
                        sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
                    .AddSingleton<IRoadNetworkSnapshotWriter>(sp =>
                        sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
                    .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => new EventSourcedEntityMap())
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                        new CommandHandlerModule[]
                        {
                            new RoadNetworkChangesArchiveCommandModule(sp.GetService<RoadNetworkUploadsBlobClient>(),
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
                    .AddScoped(sp => new TraceDbConnection<EditorContext>(
                        new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.EditorProjections)),
                        sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
                    .AddScoped(sp => new TraceDbConnection<SyndicationContext>(
                        new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.SyndicationProjections)),
                        sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
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
                                    )
                                    .Options)
                    )
                    .AddDbContext<EditorContext>((sp, options) => options
                        .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                        .UseSqlServer(
                            sp.GetRequiredService<TraceDbConnection<EditorContext>>(),
                            sqlOptions => sqlOptions
                                .UseNetTopologySuite())
                    )
                    .AddScoped(sp => new TraceDbConnection<ProductContext>(
                        new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.ProductProjections)),
                        sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
                    .AddDbContext<ProductContext>((sp, options) => options
                        .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                        .UseSqlServer(
                            sp.GetRequiredService<TraceDbConnection<ProductContext>>()));
            });
        return webHostBuilder;
    }

    public static async Task Main(string[] args)
    {
        var host = CreateWebHostBuilder(args).Build();
        var configuration = host.Services.GetRequiredService<IConfiguration>();

        var streamStore = host.Services.GetRequiredService<IStreamStore>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        try
        {
            await WaitFor.SeqToBecomeAvailable(configuration).ConfigureAwait(false);
            await WaitFor.SqlStreamStoreToBecomeAvailable(streamStore, logger).ConfigureAwait(false);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Events);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Snapshots);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.EditorProjections);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.ProductProjections);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.SyndicationProjections);

            await host.RunAsync().ConfigureAwait(false);
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
