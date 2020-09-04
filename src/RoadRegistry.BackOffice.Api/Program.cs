namespace RoadRegistry.BackOffice.Api
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using BackOffice.Framework;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
    using Be.Vlaanderen.Basisregisters.BlobStore.IO;
    using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Configuration;
    using Core;
    using Editor.Schema;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using NodaTime;
    using Product.Schema;
    using Serilog;
    using SqlStreamStore;
    using Syndication.Schema;

    public class Program
    {
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
            => new WebHostBuilder()
                .UseDefaultForApi<Startup>(
                    new ProgramOptions
                    {
                        Hosting =
                        {
                            HttpPort = 10002
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
                                if (hostContext.Configuration.GetValue<string>("MINIO_ACCESS_KEY") == null)
                                {
                                    throw new Exception("The MINIO_ACCESS_KEY configuration variable was not set.");
                                }

                                if (hostContext.Configuration.GetValue<string>("MINIO_SECRET_KEY") == null)
                                {
                                    throw new Exception("The MINIO_SECRET_KEY configuration variable was not set.");
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
                                    throw new Exception("The AWS_ACCESS_KEY_ID configuration variable was not set.");
                                }

                                if (hostContext.Configuration.GetValue<string>("AWS_SECRET_ACCESS_KEY") == null)
                                {
                                    throw new Exception("The AWS_SECRET_ACCESS_KEY configuration variable was not set.");
                                }

                                builder.AddSingleton(new AmazonS3Client(
                                        new BasicAWSCredentials(
                                            hostContext.Configuration.GetValue<string>("AWS_ACCESS_KEY_ID"),
                                            hostContext.Configuration.GetValue<string>("AWS_SECRET_ACCESS_KEY"))
                                    )
                                );
                            }

                            builder.AddSingleton<IBlobClient>(sp =>
                                new S3BlobClient(
                                    sp.GetService<AmazonS3Client>(),
                                    s3Options.Buckets[WellknownBuckets.UploadsBucket]
                                )
                            );

                            break;

                        case nameof(FileBlobClient):
                            var fileOptions = new FileBlobClientOptions();
                            hostContext.Configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);

                            builder.AddSingleton<IBlobClient>(sp =>
                                new FileBlobClient(
                                    new DirectoryInfo(fileOptions.Directory)
                                )
                            );
                            break;

                        default:
                            throw new Exception(blobOptions.BlobClientType + " is not a supported blob client type.");
                    }

                    builder
                        .AddSingleton<IStreamStore>(sp =>
                            new MsSqlStreamStoreV3(
                                new MsSqlStreamStoreV3Settings(
                                    hostContext.Configuration.GetConnectionString(WellknownConnectionNames.Events))
                                {
                                    Schema = WellknownSchemas.EventSchema
                                }))
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddSingleton(new RecyclableMemoryStreamManager())
                        .AddSingleton(sp => new RoadNetworkSnapshotReaderWriter(
                            new SqlBlobClient(
                                new SqlConnectionStringBuilder(
                                    hostContext.Configuration.GetConnectionString(WellknownConnectionNames.Snapshots)),
                                WellknownSchemas.SnapshotSchema),
                            sp.GetService<RecyclableMemoryStreamManager>()))
                        .AddSingleton<IRoadNetworkSnapshotReader>(sp =>
                            sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
                        .AddSingleton<IRoadNetworkSnapshotWriter>(sp =>
                            sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
                        .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                            new CommandHandlerModule[]
                            {
                                new RoadNetworkChangesArchiveCommandModule(
                                    sp.GetService<IBlobClient>(),
                                    sp.GetService<IStreamStore>(),
                                    sp.GetService<IRoadNetworkSnapshotReader>(),
                                    new ZipArchiveValidator(Encoding.GetEncoding(1252)),
                                    sp.GetService<IClock>()
                                ),
                                new RoadNetworkCommandModule(
                                    sp.GetService<IStreamStore>(),
                                    sp.GetService<IRoadNetworkSnapshotReader>(),
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
                                        ).Options)
                        )
                        .AddDbContext<EditorContext>((sp, options) => options
                            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                            .UseSqlServer(
                                sp.GetRequiredService<TraceDbConnection<EditorContext>>())
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
    }
}
