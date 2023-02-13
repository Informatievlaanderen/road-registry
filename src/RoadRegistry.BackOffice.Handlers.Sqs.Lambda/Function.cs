using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using System.Diagnostics;
using System.Reflection;
using Abstractions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackOffice.Extensions;
using BackOffice.Infrastructure.Modules;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
using Framework;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.Modules;
using Infrastructure.Extensions;
using Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using Environments = Be.Vlaanderen.Basisregisters.Aws.Lambda.Environments;

public sealed class Function : FunctionBase
{
    private static readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    public Function()
        : base(new List<Assembly> { typeof(BackOffice.Handlers.Sqs.DomainAssemblyMarker).Assembly, typeof(RoadRegistry.BackOffice.Abstractions.BlobRequest).Assembly })
    {
    }

    private IConfiguration BuildConfiguration(IHostEnvironment hostEnvironment)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .UseDefaultConfiguration(hostEnvironment);

        if (Debugger.IsAttached)
        {
            var dir = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            configurationBuilder
                .SetBasePath(dir);
        }

        return configurationBuilder.Build();
    }

    protected override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IHostEnvironment>(sp => new HostingEnvironment
        {
            ApplicationName = "RoadRegistry.BackOffice.Handlers.Sqs.Lambda",
            EnvironmentName = Environments.Production
        });

        var tempProvider = services.BuildServiceProvider();
        var hostEnvironment = tempProvider.GetRequiredService<IHostEnvironment>();

        var configuration = BuildConfiguration(hostEnvironment);

        var eventSourcedEntityMap = new EventSourcedEntityMap();

        services //NOSONAR logging configuration is safe
            .AddSingleton(ApplicationMetadata)
            .AddTicketing()
            .AddSingleton<IStreetNameCache, StreetNameCache>()
            .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => eventSourcedEntityMap)
            .AddStreamStore()
            .AddDistributedStreamStoreLockOptions()
            .AddRoadNetworkCommandQueue()
            .AddEditorContext()
            .AddCommandHandlerDispatcher(sp => Resolve.WhenEqualToMessage(
                new CommandHandlerModule[]
                {
                    CommandModules.RoadNetwork(sp)
                }))
                .AddLogging(configure =>
                {
                    configure.AddRoadRegistryLambdaLogger();
                })
            
            ;

        var builder = new ContainerBuilder();
        builder.RegisterConfiguration(configuration);
        builder.Populate(services);

        ConfigureContainer(builder, configuration);

        return new AutofacServiceProvider(builder.Build());
    }
    
    private void ConfigureContainer(ContainerBuilder builder, IConfiguration configuration)
    {
        var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        JsonConvert.DefaultSettings = () => eventSerializerSettings;

        builder
            .RegisterMediator();

        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder.RegisterRetryPolicy(configuration);

        builder
            .RegisterModule(new DataDogModule(configuration))
            .RegisterModule<EnvelopeModule>()
            .RegisterModule(new EventHandlingModule(typeof(BackOffice.Handlers.DomainAssemblyMarker).Assembly, eventSerializerSettings))
            .RegisterModule<RoadNetworkSnapshotModule>()
            .RegisterModule<CommandHandlingModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<SyndicationModule>()
            .RegisterModule<SqsHandlersModule>()
            .RegisterModule<BlobClientModule>()
            ;
;

        builder.RegisterIdempotentCommandHandler();
    }
}
