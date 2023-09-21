namespace RoadRegistry.Hosts;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
using Infrastructure.Extensions;
using Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Environments = Be.Vlaanderen.Basisregisters.Aws.Lambda.Environments;

public abstract class RoadRegistryLambdaFunction<TMessageHandler> : FunctionBase
    where TMessageHandler : IMessageHandler
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);
    protected readonly JsonSerializerSettings EventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    protected abstract string ApplicationName { get; }

    protected RoadRegistryLambdaFunction(IReadOnlyCollection<Assembly> messageAssemblies)
        : base(messageAssemblies, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())
    {
    }
    
    protected virtual IConfiguration BuildConfiguration(IHostEnvironment hostEnvironment)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .UseDefaultConfiguration(hostEnvironment);

        if (Debugger.IsAttached)
        {
            var dir = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)!;
            configurationBuilder.SetBasePath(dir);
        }

        return configurationBuilder.Build();
    }

    protected override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        JsonConvert.DefaultSettings = () => EventSerializerSettings;

        var hostEnvironment = new HostingEnvironment
        {
            ApplicationName = ApplicationName,
            EnvironmentName = Debugger.IsAttached ? "Development" : Environments.Production,
            ContentRootPath = Directory.GetCurrentDirectory(),
            ContentRootFileProvider = new NullFileProvider()
        };
        services.AddEmailClient();
        services.AddSingleton<IHostEnvironment>(hostEnvironment);

        var configuration = BuildConfiguration(hostEnvironment);
        var context = new HostBuilderContext(new Dictionary<object, object>())
        {
            HostingEnvironment = hostEnvironment,
            Configuration = configuration
        };

        ConfigureDefaultServices(context, services);
        ConfigureServices(context, services);

        var builder = new ContainerBuilder();
        builder.RegisterConfiguration(configuration);
        builder.Populate(services);

        ConfigureDefaultContainer(context, builder);
        ConfigureContainer(context, builder);

        var sp = new AutofacServiceProvider(builder.Build());

        Initialize(sp).ConfigureAwait(false).GetAwaiter().GetResult();

        return sp;
    }

    protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
    }
    protected virtual void ConfigureContainer(HostBuilderContext context, ContainerBuilder builder)
    {
    }

    private void ConfigureDefaultServices(HostBuilderContext context, IServiceCollection services)
    {
        services
            .AddTicketing()
            .AddSingleton(ApplicationMetadata)
            .AddSingleton(_ => new EventSourcedEntityMap())
            .AddSingleton(FileEncoding.WindowsAnsi)
            .AddEditorContext()
            .AddStreamStore()
            .AddLogging(configure => { configure.AddRoadRegistryLambdaLogger(); })
            .AddSqsLambdaHandlerOptions()
            .AddRoadRegistrySnapshot()
            .AddFeatureToggles<ApplicationFeatureToggle>(context.Configuration)
            ;
    }

    private void ConfigureDefaultContainer(HostBuilderContext context, ContainerBuilder builder)
    {
        builder
            .RegisterAssemblyTypes(typeof(TMessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder
            .RegisterMediator()
            .RegisterRetryPolicy()
            .RegisterModule(new DataDogModule(context.Configuration))
            .RegisterModule<EnvelopeModule>()
            .RegisterModule<BlobClientModule>();

        builder.RegisterType<OptionsValidator>();
    }

    protected virtual async Task Initialize(IServiceProvider sp)
    {
        var environment = sp.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment())
        {
            await sp.CreateMissingBucketsAsync(CancellationToken.None).ConfigureAwait(false);
        }

        var logger = sp.GetRequiredService<ILogger<RoadRegistryLambdaFunction<TMessageHandler>>>();
        var configuration = sp.GetRequiredService<IConfiguration>();
        logger.LogKnownSqlServerConnectionStrings(configuration);

        using (var scope = sp.CreateScope())
        {
            var optionsValidator = scope.ServiceProvider.GetRequiredService<OptionsValidator>();
            optionsValidator.ValidateAndThrow();
        }
    }
}
