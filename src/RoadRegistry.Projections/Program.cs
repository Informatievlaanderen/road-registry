namespace RoadRegistry.Projections.Shape
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.OwnedInstances;
    using Destructurama;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using SqlStreamStore;

    public class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting RoadRegistry.Projections.Shape");
            var cancellationToken = CancellationTokenSource.Token;
            cancellationToken.Register(() => Closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => CancellationTokenSource.Cancel();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.", AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var services = new ServiceCollection();
            var app = ConfigureServices(services, configuration);
            var logger = app.GetService<ILogger<Program>>();

            MigrationsHelper.Run(
                configuration.GetConnectionString("ShapeProjectionsAdmin"),
                app.GetService<ILoggerFactory>());

            try
            {
                using (var runner = app.GetService<RoadShapeRunner>())
                {
                    runner.CatchupPageSize = 5000;
                    await runner.StartAsync(
                        app.GetService<IStreamStore>(),
                        app.GetService<Func<Owned<ShapeContext>>>(),
                        cancellationToken);

                    Console.WriteLine("Running... Press CTRL + C to exit.");
                    Closing.WaitOne();
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
            Closing.Close();
        }

        private static IServiceProvider ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var app = services
                .AddLogging(s => ConfigureLogging(configuration, s))
                .BuildServiceProvider();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ShapeRunnerModule(configuration, services, app.GetService<ILoggerFactory>()));
            return new AutofacServiceProvider(builder.Build());
        }

        private static void ConfigureLogging(
            IConfiguration configuration,
            ILoggingBuilder logging)
        {
            Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Destructure.JsonNetTypes();

            //var toggles = app.ApplicationServices.GetService<IOptionsSnapshot<TogglesConfiguration>>().Value;
            //if (toggles.LogToElasticSearch)
            //    loggerConfiguration = loggerConfiguration.WriteTo.Elasticsearch(app.ApplicationServices.GetService<WegwijsElasticSearchSinkOptions>());

            var logger = Log.Logger = loggerConfiguration.CreateLogger();

            logging.AddSerilog(logger);
        }
    }
}
