namespace RoadRegistry.Hosts;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackOffice;
using BackOffice.Configuration;
using BackOffice.Extensions;
using BackOffice.FeatureToggles;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Infrastructure.Extensions;
using Infrastructure.Modules;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using RoadRegistry.Hosts.Infrastructure.HealthChecks;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class RoadRegistryHostBuilder<T> : HostBuilder
{
    private readonly string[] _args;
    private Func<IServiceProvider, Task> _runCommandDelegate;
    
    private RoadRegistryHostBuilder()
    {
        AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.", AppDomain.CurrentDomain.FriendlyName);

        AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

        ConfigureDefaultHostConfiguration()
            .ConfigureDefaultAppConfiguration()
            .ConfigureDefaultLogging()
            .ConfigureDefaultServices()
            .ConfigureDefaultContainer();
    }

    public RoadRegistryHostBuilder(string[] args) : this()
    {
        _args = args;
    }

    public new RoadRegistryHost<T> Build()
    {
        UseServiceProviderFactory(new AutofacServiceProviderFactory());
        var app = base.Build();

        var host = new RoadRegistryHost<T>(app, _runCommandDelegate);
        return host;
    }

    public new RoadRegistryHostBuilder<T> ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        base.ConfigureAppConfiguration(configureDelegate.Invoke);
        return this;
    }

    public RoadRegistryHostBuilder<T> ConfigureCommandDispatcher(Func<IServiceProvider, CommandHandlerResolver> configureDelegate)
    {
        base.ConfigureServices((hostContext, services) =>
        {
            services
                .AddSingleton(sp =>
                {
                    var resolver = configureDelegate.Invoke(sp);
                    return Dispatch.Using(resolver);
                });
        });
        return this;
    }

    public RoadRegistryHostBuilder<T> ConfigureContainer(Action<HostBuilderContext, ContainerBuilder> configureDelegate)
    {
        ConfigureContainer<ContainerBuilder>(configureDelegate.Invoke);
        return this;
    }

    public new RoadRegistryHostBuilder<T> ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        base.ConfigureHostConfiguration(configureDelegate.Invoke);
        return this;
    }

    public RoadRegistryHostBuilder<T> ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> configureDelegate)
    {
        HostingHostBuilderExtensions.ConfigureLogging(this, configureDelegate.Invoke);
        return this;
    }

    public RoadRegistryHostBuilder<T> ConfigureValidateOptions<TOptions>(Action<TOptions> validateOptions)
    {
        base.ConfigureServices((_, services) =>
        {
            services.AddOptionsValidator(validateOptions);
        });
        return this;
    }

    public new RoadRegistryHostBuilder<T> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        base.ConfigureServices(configureDelegate);
        return this;
    }

    public RoadRegistryHostBuilder<T> ConfigureHealthChecks(int hostingPort, Action<HealthCheckInitializer> configureDelegate)
    {
        base.ConfigureServices((hostContext, services) =>
        {
            var builder = services.AddHealthChecks();

            var useHealthChecksFeatureToggle = hostContext.Configuration.GetFeatureToggles<ApplicationFeatureToggle>().OfType<UseHealthChecksFeatureToggle>().Single();
            if (useHealthChecksFeatureToggle.FeatureEnabled)
            {
                configureDelegate?.Invoke(HealthCheckInitializer.Configure(builder, hostContext.Configuration, hostContext.HostingEnvironment));
            }
        });

        //this.ConfigureWebHostDefaults(webHostBuilder =>
        //    webHostBuilder
        //        .UseStartup<RoadRegistryHostStartup>()
        //        .UseKestrel((context, builder) =>
        //        {
        //            if (context.HostingEnvironment.IsDevelopment())
        //            {
        //                builder.ListenLocalhost(hostingPort);
        //            }
        //        })
        //);

        return this;
    }

    private RoadRegistryHostBuilder<T> ConfigureDefaultHostConfiguration()
    {
        return ConfigureHostConfiguration(services =>
        {
            services
                .AddEnvironmentVariables("DOTNET_")
                .AddEnvironmentVariables("ASPNETCORE_");
        });
    }

    private RoadRegistryHostBuilder<T> ConfigureDefaultAppConfiguration()
    {
        return ConfigureAppConfiguration((hostContext, services) =>
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (hostContext.HostingEnvironment.IsProduction())
            {
                services
                    .SetBasePath(Directory.GetCurrentDirectory());
            }

            if (hostContext.HostingEnvironment.IsDevelopment())
            {
                Environment.SetEnvironmentVariable("AWS_REGION", "eu-west-1");
            }

            services
                .AddJsonFile("appsettings.json", true, false)
                .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName.ToLowerInvariant()}.json", true, false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, false)
                .AddEnvironmentVariables()
                .AddCommandLine(_args);
        });
    }

    private RoadRegistryHostBuilder<T> ConfigureDefaultLogging()
    {
        return ConfigureLogging((hostContext, builder) =>
        {
            builder.AddSerilog<T>(hostContext.Configuration);
        });
    }

    private RoadRegistryHostBuilder<T> ConfigureDefaultServices()
    {
        return ConfigureServices((hostContext, services) =>
        {
            services
                .AddSingleton<Scheduler>()
                .AddStreamStore()
                .AddSingleton<IClock>(SystemClock.Instance)
                .AddSingleton(new RecyclableMemoryStreamManager())
                .AddFeatureToggles<ApplicationFeatureToggle>(hostContext.Configuration)
                .AddSingleton(new WKTReader(
                    new NtsGeometryServices(
                        GeometryConfiguration.GeometryFactory.PrecisionModel,
                        GeometryConfiguration.GeometryFactory.SRID
                    )
                ))
                .AddSingleton(FileEncoding.WindowsAnsi);
        });
    }

    private RoadRegistryHostBuilder<T> ConfigureDefaultContainer()
    {
        return ConfigureContainer((context, builder) =>
        {
            builder
                .RegisterMediator()
                .RegisterType<OptionsValidator>();

            var blobClientOptions = context.Configuration.GetOptions<BlobClientOptions>();
            if (blobClientOptions.BlobClientType is not null)
            {
                builder.RegisterModule<BlobClientModule>();
            }
        });
    }

    public RoadRegistryHostBuilder<T> ConfigureRunCommand(Func<IServiceProvider, CancellationToken, Task> runCommandDelegate)
    {
        return ConfigureRunCommand(sp =>
        {
            var hostApplicationLifetime = sp.GetRequiredService<IHostApplicationLifetime>();
            return runCommandDelegate(sp, hostApplicationLifetime.ApplicationStopping);
        });
    }
    public RoadRegistryHostBuilder<T> ConfigureRunCommand(Func<IServiceProvider, Task> runCommandDelegate)
    {
        _runCommandDelegate = runCommandDelegate;
        return this;
    }
}
