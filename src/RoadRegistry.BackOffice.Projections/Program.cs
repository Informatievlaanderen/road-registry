namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.OwnedInstances;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Destructurama;
    using Framework;
    using Messages;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using Model;
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

                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
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
                    builder
                        .AddSingleton(new EventDeserializer((data, type) =>
                            JsonConvert.DeserializeObject(data, type, SerializerSettings)))
                        .AddSingleton(sp =>
                            new MsSqlStreamStore(
                                new MsSqlStreamStoreSettings(sp.GetService<IConfiguration>()
                                    .GetConnectionString("Events")) {Schema = "RoadRegistry"}))
                        .AddSingleton<IStreamStore>(sp => sp.GetRequiredService<MsSqlStreamStore>())
                        .AddSingleton<IReadonlyStreamStore>(sp => sp.GetRequiredService<MsSqlStreamStore>())
                        .AddSingleton<IBlobClient>(sp =>
                            new SqlBlobClient(
                                new SqlConnectionStringBuilder(sp.GetService<IConfiguration>()
                                    .GetConnectionString("Blobs")), "RoadRegistryBlobs"))
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddSingleton(new RecyclableMemoryStreamManager())
                        .AddSingleton(sp => new RoadShapeRunner(
                            new EnvelopeFactory(EventMapping, sp.GetService<EventDeserializer>()),
                            sp.GetService<ILoggerFactory>(),
                            sp.GetService<IBlobClient>(),
                            sp.GetService<RecyclableMemoryStreamManager>()))
                        .AddDbContext<ShapeContext>((sp, options) => options
                            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                            .UseSqlServer(sp.GetService<IConfiguration>().GetConnectionString("ShapeProjections"),
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
                    configuration.GetConnectionString("ShapeProjectionsAdmin"),
                    host.Services.GetService<ILoggerFactory>());

                using (var source = new CancellationTokenSource())
                {
                    try
                    {
                        runner.CatchupPageSize = 200_000;

                        await runner.StartAsync(streamStore, host.Services.GetService<Func<Owned<ShapeContext>>>(), source.Token);
                        await host.RunAsync();
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
                Log.CloseAndFlush();
                // Allow some time for flushing before shutdown.
                Thread.Sleep(1000);
                throw;
            }

            Console.WriteLine("\nStopping...");
        }

        private class Disposable : IDisposable
        {
            public static readonly IDisposable Disposed = new Disposable();
            public void Dispose()
            {
            }
        }
    }
}
