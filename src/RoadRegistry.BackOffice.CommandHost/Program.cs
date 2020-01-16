namespace RoadRegistry.BackOffice.CommandHost
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Destructurama;
    using Framework;
    using Messages;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IO;
    using Model;
    using NodaTime;
    using Serilog;
    using SqlStreamStore;
    using Translation;

    public class Program
    {
        private static readonly string Schema = "RoadRegistryCommandHost";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting RoadRegistry.BackOffice.CommandHost");

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
                .ConfigureServices((hostContext, builder) =>
                {
                    builder
                        .AddSingleton<Scheduler>()
                        .AddHostedService<CommandProcessor>()
                        .AddSingleton<ICommandProcessorPositionStore>(sp =>
                            new SqlCommandProcessorPositionStore(
                                new SqlConnectionStringBuilder(
                                    sp.GetService<IConfiguration>().GetConnectionString("CommandHost")
                                ),
                                Schema))
                        .AddSingleton<IStreamStore>(sp =>
                            new MsSqlStreamStore(
                                new MsSqlStreamStoreSettings(sp.GetService<IConfiguration>()
                                    .GetConnectionString("Events")) {Schema = "RoadRegistry"}))
                        .AddSingleton<IBlobClient>(sp =>
                            new SqlBlobClient(
                                new SqlConnectionStringBuilder(sp.GetService<IConfiguration>()
                                    .GetConnectionString("Blobs")), "RoadRegistryBlobs"))
                        .AddSingleton<IClock>(SystemClock.Instance)
                        .AddSingleton(new RecyclableMemoryStreamManager())
                        .AddSingleton(sp => new RoadNetworkSnapshotReaderWriter(sp.GetService<IBlobClient>(), sp.GetService<RecyclableMemoryStreamManager>()))
                        .AddSingleton<IRoadNetworkSnapshotReader>(sp => sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
                        .AddSingleton<IRoadNetworkSnapshotWriter>(sp => sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
                        .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                            new CommandHandlerModule[]
                            {
                                new RoadNetworkChangesArchiveCommandModule(
                                    sp.GetService<IBlobClient>(),
                                    sp.GetService<IStreamStore>(),
                                    sp.GetService<IRoadNetworkSnapshotReader>(),
                                    new ZipArchiveValidator(Encoding.UTF8),
                                    sp.GetService<IClock>()
                                ),
                                new RoadNetworkCommandModule(
                                    sp.GetService<IStreamStore>(),
                                    sp.GetService<IRoadNetworkSnapshotReader>(),
                                    sp.GetService<IClock>()
                                )
                            })));
                })
                .Build();

            var configuration = host.Services.GetService<IConfiguration>();
            var streamStore = host.Services.GetService<IStreamStore>();
            var logger = host.Services.GetService<ILogger<Program>>();

            try
            {
                await streamStore.WaitUntilAvailable();
                await
                    new SqlCommandProcessorPositionStoreSchema(
                            new SqlConnectionStringBuilder(configuration.GetConnectionString("CommandHostAdmin"))
                    ).CreateSchemaIfNotExists(Schema);
                await host.RunAsync();
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
    }
}
