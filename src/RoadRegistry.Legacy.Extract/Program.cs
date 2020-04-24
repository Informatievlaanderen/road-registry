namespace RoadRegistry.Legacy.Extract
{
    using System;
    using System.Data;
    using Microsoft.Data.SqlClient;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
    using Be.Vlaanderen.Basisregisters.BlobStore.IO;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using NodaTime;
    using Readers;
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

                    builder
                        .AddSingleton<WellKnownBinaryReader>()
                        .AddSingleton<RecyclableMemoryStreamManager>()
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddSingleton<IEventReader, LegacyEventReader>()
                        .AddSingleton<LegacyStreamArchiveWriter>()
                        .AddSingleton(
                            new SqlConnection(
                                hostContext.Configuration.GetConnectionString(WellknownConnectionNames.Legacy)
                            )
                        );
                })
                .Build();

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var reader = host.Services.GetRequiredService<IEventReader>();
            var writer = host.Services.GetRequiredService<LegacyStreamArchiveWriter>();
            var blobClient = host.Services.GetRequiredService<IBlobClient>();
            var blobClientOptions = new BlobClientOptions();
            configuration.Bind(blobClientOptions);

            try
            {
                await WaitFor.SeqToBecomeAvailable(configuration);

                logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Legacy);
                logger.LogBlobClientCredentials(blobClientOptions);

                await WaitFor.SqlServerToBecomeAvailable(
                    new SqlConnectionStringBuilder(configuration.GetConnectionString(WellknownConnectionNames.Legacy))
                    , logger);

                await OptimizeDatabasePerformance(
                    new SqlConnectionStringBuilder(
                        configuration.GetConnectionString(WellknownConnectionNames.Legacy)), logger);

                await blobClient.ProvisionResources(host);

                using (var connection = host.Services.GetRequiredService<SqlConnection>())
                {
                    await connection.OpenAsync();

                    await writer.WriteAsync(reader.ReadEvents(connection));
                }
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

        private static async Task OptimizeDatabasePerformance(SqlConnectionStringBuilder builder,
            ILogger<Program> logger, CancellationToken token = default)
        {
            logger.LogInformation("Optimizing database for performance ...");
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync(token).ConfigureAwait(false);
                using (var command = new SqlCommand(@"
IF NOT EXISTS(SELECT * FROM [sys].[indexes] WHERE name='IX_WB_WS' AND object_id = OBJECT_ID('dbo.wegbreedte')) BEGIN CREATE INDEX [IX_WB_WS] ON [dbo].[wegbreedte] ([wegsegmentID]) END
IF NOT EXISTS(SELECT * FROM [sys].[indexes] WHERE name='IX_RS_WS' AND object_id = OBJECT_ID('dbo.rijstroken')) BEGIN CREATE INDEX [IX_RS_WS] ON [dbo].[rijstroken] ([wegsegmentID]) END
IF NOT EXISTS(SELECT * FROM [sys].[indexes] WHERE name='IX_WV_WS' AND object_id = OBJECT_ID('dbo.wegverharding')) BEGIN CREATE INDEX [IX_WV_WS] ON [dbo].[wegverharding] ([wegsegmentID]) END", connection))
                {
                    command.CommandType = CommandType.Text;
                    await command.ExecuteNonQueryAsync(token);
                }
            }
        }
    }
}
