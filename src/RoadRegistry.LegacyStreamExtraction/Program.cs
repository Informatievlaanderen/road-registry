namespace RoadRegistry.LegacyStreamExtraction
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
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
                                    s3Options.OutputBucket
                                )
                            );
                            break;
                        case nameof(FileBlobClient):
                            var fileOptions = new FileBlobClientOptions();
                            hostContext.Configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);

                            builder.AddSingleton<IBlobClient>(sp =>
                                new FileBlobClient(
                                    new DirectoryInfo(fileOptions.OutputDirectory)
                                )
                            );
                            break;
                    }

                    var legacyDatabaseOptions = new LegacySqlDatabaseOptions();
                    hostContext.Configuration.GetSection(nameof(LegacySqlDatabaseOptions)).Bind(legacyDatabaseOptions);

                    builder
                        .AddSingleton<WellKnownBinaryReader>()
                        .AddSingleton<RecyclableMemoryStreamManager>()
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddSingleton<IEventReader, LegacyEventReader>()
                        .AddSingleton<LegacyStreamArchiveWriter>()
                        .AddSingleton(
                            new SqlConnection(
                                hostContext.Configuration.GetConnectionString(legacyDatabaseOptions.ConnectionStringName)
                            )
                        );
                })
                .Build();

            var configuration = host.Services.GetService<IConfiguration>();
            var logger = host.Services.GetService<ILogger<Program>>();
            var reader = host.Services.GetService<IEventReader>();
            var writer = host.Services.GetService<LegacyStreamArchiveWriter>();

            try
            {
                var legacyDatabaseOptions = new LegacySqlDatabaseOptions();
                configuration.GetSection(nameof(LegacySqlDatabaseOptions)).Bind(legacyDatabaseOptions);

                await WaitForSqlServer(
                    new SqlConnectionStringBuilder(
                        configuration.GetConnectionString(legacyDatabaseOptions.ConnectionStringName)), logger);

                using (var connection = host.Services.GetService<SqlConnection>())
                {
                    await connection.OpenAsync();

                    await writer.WriteAsync(reader.ReadEvents(connection));
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
