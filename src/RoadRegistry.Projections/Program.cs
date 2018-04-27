namespace RoadRegistry.Projections.Oslo
{
    using System;
    using System.IO;
    using System.Threading;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.OwnedInstances;
    using Destructurama;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using SqlStreamStore;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting RoadRegistry.Projections.Oslo");

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.", AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var services = new ServiceCollection();
            var app = ConfigureServices(services, configuration);
            var logger = app.GetService<ILogger<Program>>();

            MigrationsHelper.Run(configuration.GetConnectionString("OsloProjectionsAdmin"), app.GetService<ILoggerFactory>());

            try
            {
                app.GetService<RoadOsloRunner>()
                    .Handle(app.GetService<IStreamStore>(), app.GetService<Func<Owned<OsloContext>>>());

                Console.WriteLine("Running...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();
                // Allow some time for flushing before shutdown.
                Thread.Sleep(1000);
                throw;
            }
        }

        private static IServiceProvider ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var app = services
                .AddLogging(s => ConfigureLogging(configuration, s))
                .BuildServiceProvider();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new OsloRunnerModule(configuration, services, app.GetService<ILoggerFactory>()));
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
