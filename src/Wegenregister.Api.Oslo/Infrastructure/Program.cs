namespace Wegenregister.Api.Oslo.Infrastructure
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using Aiv.Vbr.AspNetCore.Mvc.Formatters.Json;
    using Aiv.Vbr.Configuration.Database;
    using Destructurama;
    using Wegenregister.Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

            JsonConvert.DefaultSettings =
                () => JsonSerializerSettingsProvider.CreateSerializerSettings().ConfigureDefaultForApi();

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.", AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            CreateWebHostBuilder(args)
                .Build()
                .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            IWebHostBuilder hostBuilder = new WebHostBuilder();
            var environment = hostBuilder.GetSetting("environment");

            if (environment == "Development")
            {
                var cert = new X509Certificate2("api.dev.weg.basisregisters.vlaanderen.be.pfx", "gemeenteregister!");

                hostBuilder = hostBuilder
                    .UseKestrel(options =>
                    {
                        options.AddServerHeader = false;

                        // Map localhost to api.dev.weg.basisregisters.vlaanderen.be
                        // Then use https://api.dev.weg.basisregisters.vlaanderen.be:2443 in a browser
                        options.Listen(new IPEndPoint(IPAddress.Loopback, 2443), listenOptions => listenOptions.UseConnectionLogging().UseHttps(cert));
                        options.Listen(new IPEndPoint(IPAddress.Loopback, 2080), listenOptions => listenOptions.UseConnectionLogging());
                    });
            }
            else
            {
                hostBuilder = hostBuilder.UseKestrel(server => server.AddServerHeader = false);
            }

            return hostBuilder
                .CaptureStartupErrors(true)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseWebRoot("wwwroot")
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config
                        .SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);

                    var sqlConfiguration =
                        config
                            .Build()
                            .GetSection(ConfigurationDatabaseConfiguration.Section)
                            .Get<ConfigurationDatabaseConfiguration>();

                    config
                        .AddEntityFramework(
                            x => x.UseSqlServer(
                                sqlConfiguration.ConnectionString,
                                sqlServerOptions =>
                                {
                                    sqlServerOptions.EnableRetryOnFailure();
                                    sqlServerOptions.MigrationsHistoryTable(ConfigurationMigrationsTableInfo.DefaultMigrationsTableName, Schema.Default);
                                }),
                            new ConfigurationTableInfo(Schema.Default));
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

                    var loggerConfiguration = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostingContext.Configuration)
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
                })
                .UseStartup<Startup>();
        }
    }
}
