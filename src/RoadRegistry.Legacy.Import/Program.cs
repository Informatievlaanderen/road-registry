namespace RoadRegistry.Legacy.Import
{
    using System;
    using Microsoft.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using System.Text;
    using System.Threading;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using BackOffice.Core;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
    using Be.Vlaanderen.Basisregisters.BlobStore.IO;
    using Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;

    public class Program
    {
        private static async Task Main(string[] args)
        {
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
                        .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                        .Build();
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
                                    sp.GetRequiredService<AmazonS3Client>(),
                                    s3Options.Buckets[WellknownBuckets.ImportLegacyBucket]
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
                    }

                    var legacyStreamArchiveReader = new LegacyStreamArchiveReader(
                        new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            DateFormatHandling = DateFormatHandling.IsoDateFormat,
                            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                            DateParseHandling = DateParseHandling.DateTime,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        }
                    );

                    builder
                        .AddSingleton(legacyStreamArchiveReader)
                        .AddSingleton(
                            new SqlConnection(
                                hostContext.Configuration.GetConnectionString(WellknownConnectionNames.Events)
                            )
                        )
                        .AddSingleton<IStreamStore>(new MsSqlStreamStore(
                            new MsSqlStreamStoreSettings(
                                hostContext.Configuration.GetConnectionString(WellknownConnectionNames.Events)
                            )
                            {
                                Schema = WellknownSchemas.EventSchema
                            }))
                        .AddSingleton<LegacyStreamEventsWriter>();
                })
                .Build();

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var reader = host.Services.GetRequiredService<LegacyStreamArchiveReader>();
            var writer = host.Services.GetRequiredService<LegacyStreamEventsWriter>();
            var client = host.Services.GetRequiredService<IBlobClient>();
            var streamStore = host.Services.GetRequiredService<IStreamStore>();
            var blobClientOptions = new BlobClientOptions();
            configuration.Bind(blobClientOptions);

            try
            {
                await WaitFor.SeqToBecomeAvailable(configuration).ConfigureAwait(false);

                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Events);
                logger.LogBlobClientCredentials(blobClientOptions);

                await DistributedLock<Program>.RunAsync(async () =>
                    {
                        var eventsConnectionStringBuilder =
                            new SqlConnectionStringBuilder(
                                configuration.GetConnectionString(WellknownConnectionNames.Events));
                        var masterConnectionStringBuilder =
                            new SqlConnectionStringBuilder(eventsConnectionStringBuilder.ConnectionString)
                            {
                                InitialCatalog = "master"
                            };

                        await WaitFor.SqlServerToBecomeAvailable(masterConnectionStringBuilder, logger).ConfigureAwait(false);
                        await WaitFor.SqlServerDatabaseToBecomeAvailable(
                            masterConnectionStringBuilder,
                            eventsConnectionStringBuilder,
                            logger
                        ).ConfigureAwait(false);

                        if (streamStore is MsSqlStreamStore sqlStreamStore)
                        {
                            await sqlStreamStore.CreateSchema().ConfigureAwait(false);
                        }

                        var page = await streamStore
                            .ReadStreamForwards(RoadNetworks.Stream, StreamVersion.Start, 1)
                            .ConfigureAwait(false);
                        if (page.Status == PageReadStatus.StreamNotFound)
                        {
                            var blob = await client
                                .GetBlobAsync(new BlobName("import-streams.zip"), CancellationToken.None)
                                .ConfigureAwait(false);

                            var watch = Stopwatch.StartNew();
                            using (var blobStream = await blob.OpenAsync().ConfigureAwait(false))
                            {
                                await writer
                                    .WriteAsync(reader.Read(blobStream))
                                    .ConfigureAwait(false);
                            }

                            logger.LogInformation("Total append took {0}ms", watch.ElapsedMilliseconds);
                        }
                        else
                        {
                            logger.LogWarning(
                                "The road network was previously imported. This can only be performed once.");
                        }
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration),
                    logger)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "Encountered a fatal exception, exiting program.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
