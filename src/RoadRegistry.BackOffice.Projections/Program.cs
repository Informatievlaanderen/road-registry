namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.OwnedInstances;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
    using Be.Vlaanderen.Basisregisters.BlobStore.IO;
    using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Configuration;
    using Destructurama;
    using Framework;
    using Messages;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using Newtonsoft.Json;
    using NodaTime;
    using Schema;
    using Serilog;
    using SqlStreamStore;

    public class Program
    {
        private static readonly JsonSerializerSettings SerializerSettings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        private static readonly EventMapping EventMapping =
            new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting RoadRegistry.BackOffice.Projections");

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception) eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    if (hostContext.HostingEnvironment.IsProduction())
                    {
                        builder
                            .SetBasePath(Directory.GetCurrentDirectory());
                    }

                    builder
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
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
                        .Enrich.WithEnvironmentUserName()
                        .Destructure.JsonNetTypes();

                    Log.Logger = loggerConfiguration.CreateLogger();

                    builder.AddSerilog(Log.Logger);
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
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
                                    s3Options.BucketPrefix + WellknownBuckets.UploadsBucket
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
                        .AddSingleton(new EventDeserializer((data, type) =>
                            JsonConvert.DeserializeObject(data, type, SerializerSettings)))
                        .AddSingleton(sp =>
                            new MsSqlStreamStore(
                                new MsSqlStreamStoreSettings(
                                    sp
                                        .GetService<IConfiguration>()
                                        .GetConnectionString(WellknownConnectionNames.Events))
                                {
                                    Schema = WellknownSchemas.EventSchema
                                }))
                        .AddSingleton<IStreamStore>(sp => sp.GetRequiredService<MsSqlStreamStore>())
                        .AddSingleton<IReadonlyStreamStore>(sp => sp.GetRequiredService<MsSqlStreamStore>())
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddSingleton(new RecyclableMemoryStreamManager())
                        .AddSingleton(sp => new RoadShapeRunner(
                            new EnvelopeFactory(EventMapping, sp.GetService<EventDeserializer>()),
                            sp.GetService<ILoggerFactory>(),
                            sp.GetService<IBlobClient>(),
                            sp.GetService<RecyclableMemoryStreamManager>()))
                        .AddDbContext<ShapeContext>((sp, options) => options
                            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                            .UseSqlServer(sp.GetService<IConfiguration>().GetConnectionString(WellknownConnectionNames.ShapeProjections),
                                sqlServerOptions =>
                                {
                                    sqlServerOptions.EnableRetryOnFailure();
                                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Shape,
                                        Schema.ProjectionMetaData);
                                }));

//                    logger.LogInformation(
//                        "Added {Context} to services:" + Environment.NewLine +
//                        "\tSchema: {Schema}" + Environment.NewLine +
//                        "\tMigrationTable: {ProjectionMetaData}.{TableName}",
//                        nameof(ShapeContext),
//                        Schema.Shape,
//                        Schema.ProjectionMetaData, MigrationTables.Shape);
                })
                .Build();

            var configuration = host.Services.GetService<IConfiguration>();
            var streamStore = host.Services.GetService<IStreamStore>();
            var logger = host.Services.GetService<ILogger<Program>>();
            var runner = host.Services.GetService<RoadShapeRunner>();

            try
            {
                await streamStore.WaitUntilAvailable();

                MigrationsHelper.Run(
                    configuration.GetConnectionString(WellknownConnectionNames.ShapeProjectionsAdmin),
                    host.Services.GetService<ILoggerFactory>());

                using (var source = new CancellationTokenSource())
                {
                    try
                    {
                        runner.CatchupPageSize = 10_000;

                        await runner.StartAsync(
                            streamStore,
                            host.Services.GetService<Func<Owned<ShapeContext>>>(),
                            source.Token);
                        await host.RunAsync(source.Token);
                    }
                    finally
                    {
                        source.Cancel();
                    }
                }
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
}
