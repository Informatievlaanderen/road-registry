namespace RoadRegistry.Hosts;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Infrastructure.Extensions;
using Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using Serilog;
using Serilog.Debugging;
using System;
using System.IO;
using System.Text;
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
        var internalHost = base.Build();

        return new RoadRegistryHost<T>(internalHost, _runCommandDelegate);
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
    
    public new RoadRegistryHostBuilder<T> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        base.ConfigureServices(configureDelegate);
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
                services
                    .SetBasePath(Directory.GetCurrentDirectory());

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
            SelfLog.Enable(Console.WriteLine);

            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(hostContext.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName();

            Log.Logger = loggerConfiguration.CreateLogger();

            builder.AddSerilog(Log.Logger);
        });
    }
    
    private RoadRegistryHostBuilder<T> ConfigureDefaultServices()
    {
        return ConfigureServices((hostContext, services) =>
        {
            services
                .AddDistributedS3Cache()
                .AddSingleton<Scheduler>()
                .AddStreamStore()
                .AddSingleton<IClock>(SystemClock.Instance)
                .AddRoadRegistrySnapshot()
                .AddSingleton(new RecyclableMemoryStreamManager())
                .AddFeatureToggles<ApplicationFeatureToggle>(hostContext.Configuration)
                .AddSingleton(new WKTReader(
                    new NtsGeometryServices(
                        GeometryConfiguration.GeometryFactory.PrecisionModel,
                        GeometryConfiguration.GeometryFactory.SRID
                    )
                ));
        });
    }

    private RoadRegistryHostBuilder<T> ConfigureDefaultContainer()
    {
        return ConfigureContainer((context, builder) =>
        {
            builder
                .RegisterMediator();

            builder.RegisterModule<BlobClientModule>();
        });
    }

    public RoadRegistryHostBuilder<T> ConfigureRunCommand(Func<IServiceProvider, Task> runCommandDelegate)
    {
        _runCommandDelegate = runCommandDelegate;
        return this;
    }
}
