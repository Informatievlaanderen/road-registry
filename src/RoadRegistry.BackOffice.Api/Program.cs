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
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using NodaTime;
    using Schema;
    using Serilog;
    using SqlStreamStore;

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
                            HttpPort = 10000
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
                            if (Environment.GetEnvironmentVariable("MINIO_SERVER") != null)
                            {
                                if (Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY") == null)
                                {
                                    throw new Exception("The MINIO_ACCESS_KEY environment variable was not set.");
                                }

                                if (Environment.GetEnvironmentVariable("MINIO_SECRET_KEY") == null)
                                {
                                    throw new Exception("The MINIO_SECRET_KEY environment variable was not set.");
                                }

                                builder.AddSingleton(new AmazonS3Client(
                                        new BasicAWSCredentials(
                                            Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY"),
                                            Environment.GetEnvironmentVariable("MINIO_SECRET_KEY")),
                                        new AmazonS3Config
                                        {
                                            RegionEndpoint = RegionEndpoint.USEast1, // minio's default region
                                            ServiceURL = Environment.GetEnvironmentVariable("MINIO_SERVER"),
                                            ForcePathStyle = true
                                        }
                                    )
                                );

                            }
                            else // Use AWS
                            {
                                if (Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") == null)
                                {
                                    throw new Exception("The AWS_ACCESS_KEY_ID environment variable was not set.");
                                }

                                if (Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") == null)
                                {
                                    throw new Exception("The AWS_SECRET_ACCESS_KEY environment variable was not set.");
                                }

                                builder.AddSingleton(new AmazonS3Client(
                                        new BasicAWSCredentials(
                                            Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
                                            Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"))
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
                            new MsSqlStreamStore(
                                new MsSqlStreamStoreSettings(
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
                        .AddScoped(sp => new TraceDbConnection<BackOfficeContext>(
                            new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.BackOfficeProjections)),
                            sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
                        .AddDbContext<BackOfficeContext>((sp, options) => options
                            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                            .UseSqlServer(
                                sp.GetRequiredService<TraceDbConnection<BackOfficeContext>>(),
                                sql => sql.EnableRetryOnFailure())
                        );
                });
    }
}
