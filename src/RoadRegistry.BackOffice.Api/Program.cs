namespace RoadRegistry.BackOffice.Api;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abstractions;
using Amazon;
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
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Core;
using Editor.Schema;
using Hosts;
using Hosts.Configuration;
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
                            if (hostContext.Configuration.GetValue<string>("AWS_ACCESS_KEY_ID") == null) throw new InvalidOperationException("The AWS_ACCESS_KEY_ID configuration variable was not set.");

                            if (hostContext.Configuration.GetValue<string>("AWS_SECRET_ACCESS_KEY") == null) throw new InvalidOperationException("The AWS_SECRET_ACCESS_KEY configuration variable was not set.");

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

                    default: throw new InvalidOperationException(blobOptions.BlobClientType + " is not a supported blob client type.");
                }

                var zipArchiveWriterOptions = new ZipArchiveWriterOptions();
                hostContext.Configuration.GetSection(nameof(ZipArchiveWriterOptions)).Bind(zipArchiveWriterOptions);
                var extractDownloadsOptions = new ExtractDownloadsOptions();
                hostContext.Configuration.GetSection(nameof(ExtractDownloadsOptions)).Bind(extractDownloadsOptions);
                var extractUploadsOptions = new ExtractUploadsOptions();
                hostContext.Configuration.GetSection(nameof(ExtractUploadsOptions)).Bind(extractDownloadsOptions);
                var featureToggles = new FeatureToggleOptions();
                hostContext.Configuration.GetSection(FeatureToggleOptions.ConfigurationKey).Bind(featureToggles);

                var sqsOptions = new SqsOptions();
                hostContext.Configuration.GetSection(nameof(SqsOptions)).Bind(sqsOptions);

                builder
                    .AddSingleton<ISqsQueuePublisher>(sp =>
                        new SqsQueuePublisher(sqsOptions, sp.GetService<ILogger<SqsQueuePublisher>>())
                    )
                    .AddSingleton<IZipArchiveBeforeFeatureCompareValidator>(sp => new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8))
                    .AddSingleton<IZipArchiveAfterFeatureCompareValidator>(sp => new ZipArchiveAfterFeatureCompareValidator(Encoding.UTF8));

                builder
                    .AddSingleton(c => new UseSnapshotRebuildFeatureToggle(featureToggles.UseSnapshotRebuildFeature))
                    .AddSingleton<ProblemDetailsHelper>()
                    .AddSingleton(zipArchiveWriterOptions)
                    .AddSingleton(extractDownloadsOptions)
                    .AddSingleton(extractUploadsOptions)
                    .AddSingleton(sqsOptions)
                    .AddSingleton<IStreamStore>(sp =>
                        new MsSqlStreamStoreV3(
                            new MsSqlStreamStoreV3Settings(
                                    hostContext.Configuration.GetConnectionString(WellknownConnectionNames.Events))
                                { Schema = WellknownSchemas.EventSchema }))
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
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                        new CommandHandlerModule[]
                        {
                            new RoadNetworkChangesArchiveCommandModule(sp.GetService<RoadNetworkFeatureCompareBlobClient>(),
                                sp.GetService<IStreamStore>(),
                                sp.GetService<IRoadNetworkSnapshotReader>(),
                                new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
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
                                new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                                sp.GetService<IClock>()
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

    public const int HostingPort = 10002;

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