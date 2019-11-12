namespace RoadRegistry.LegacyStreamLoader
{
    using System;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using System.Text;
    using System.Threading;
    using Amazon.Runtime;
    using Amazon.S3;
    using BackOffice.Model;
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
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
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
                        .WriteTo.Console()
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .Enrich.WithEnvironmentUserName();

                    Log.Logger = loggerConfiguration.CreateLogger();

                    builder.AddSerilog(Log.Logger);
                })
                .ConfigureServices((hostContext, builder) =>
                {
                    var options = new BlobClientOptions();
                    hostContext.Configuration.Bind(options);

                    switch (options.BlobClientType)
                    {
                        case nameof(S3BlobClient):
                            var s3Options = new S3BlobClientOptions();
                            hostContext.Configuration.GetSection(nameof(S3BlobClientOptions)).Bind(s3Options);

                            builder.AddSingleton(new AmazonS3Client(
                                new BasicAWSCredentials(
                                    s3Options.AwsAccessKeyId,
                                    s3Options.AwsSecretAccessKey)
                                )
                            );
                            builder.AddSingleton<IBlobClient>(sp =>
                                new S3BlobClient(
                                    sp.GetService<AmazonS3Client>(),
                                    s3Options.InputBucket
                                )
                            );
                            break;
                        case nameof(FileBlobClient):
                            var fileOptions = new FileBlobClientOptions();
                            hostContext.Configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);

                            builder.AddSingleton<IBlobClient>(sp =>
                                new FileBlobClient(
                                    new DirectoryInfo(fileOptions.InputDirectory)
                                )
                            );
                            break;
                    }

                    var eventDatabaseOptions = new EventSqlDatabaseOptions();
                    hostContext.Configuration.GetSection(nameof(EventSqlDatabaseOptions)).Bind(eventDatabaseOptions);

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
                                hostContext.Configuration.GetConnectionString(eventDatabaseOptions.ConnectionStringName)
                            )
                        )
                        .AddSingleton<IStreamStore>(new MsSqlStreamStore(
                            new MsSqlStreamStoreSettings(
                                hostContext.Configuration.GetConnectionString(eventDatabaseOptions.ConnectionStringName)
                            )
                            {
                                Schema = "RoadRegistry"
                            }))
                        .AddSingleton<LegacyStreamEventsWriter>();
                })
                .Build();

            var configuration = host.Services.GetService<IConfiguration>();
            var logger = host.Services.GetService<ILogger<Program>>();
            var reader = host.Services.GetService<LegacyStreamArchiveReader>();
            var writer = host.Services.GetService<LegacyStreamEventsWriter>();
            var client = host.Services.GetService<IBlobClient>();
            var streamStore = host.Services.GetService<IStreamStore>();

            try
            {
                var eventDatabaseOptions = new EventSqlDatabaseOptions();
                configuration.GetSection(nameof(EventSqlDatabaseOptions)).Bind(eventDatabaseOptions);

                var builder = new SqlConnectionStringBuilder(
                    configuration.GetConnectionString(eventDatabaseOptions.ConnectionStringName))
                {
                    InitialCatalog = "master"
                };

                await WaitForSqlServer(builder, logger).ConfigureAwait(false);

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
                    logger.LogWarning("The road network was previously imported. This can only be performed once.");
                }
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();
                // Allow some time for flushing before shutdown.
                Thread.Sleep(1000);
                throw;
            }
        }

        private static async Task WaitForSqlServer(SqlConnectionStringBuilder builder, ILogger<Program> logger, CancellationToken token = default)
        {
            var exit = false;
            while(!exit)
            {
                try
                {
                    logger.LogInformation("Waiting for sql server to become available");
                    using (var connection = new SqlConnection(builder.ConnectionString))
                    {
                        await connection.OpenAsync(token).ConfigureAwait(false);
                        exit = true;
                    }
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                }
            }
        }
    }
}
