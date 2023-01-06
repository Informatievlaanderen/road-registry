using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
using Infrastructure;
using Infrastructure.Modules;
using Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Infrastructure.Modules;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Hosts.Infrastructure.Modules;
using SqlStreamStore;
using TicketingService.Proxy.HttpProxy;
using DomainAssemblyMarker = BackOffice.Handlers.DomainAssemblyMarker;
using Environments = Be.Vlaanderen.Basisregisters.Aws.Lambda.Environments;

public sealed class Function : FunctionBase
{
    private static readonly CommandMetadata CommandMetadata = new(RoadRegistryApplication.Lambda);

    public Function()
        : base(new List<Assembly> { typeof(Sqs.DomainAssemblyMarker).Assembly })
    {
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

        var configuration = new ConfigurationBuilder()
            .UseDefaultConfiguration(hostEnvironment)
            .Build();

        return ConfigureServices(services, configuration);
    }

    private IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var builder = new ContainerBuilder();
        builder
            .RegisterMediator();

        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder.Register(c => configuration)
            .AsSelf()
            .As<IConfiguration>()
            .SingleInstance();

        services.AddHttpProxyTicketing(configuration.GetSection(TicketingOptions.ConfigurationKey)[nameof(TicketingOptions.InternalBaseUrl)]);

        builder.RegisterRetryPolicy(configuration);

        var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        JsonConvert.DefaultSettings = () => eventSerializerSettings;

        builder
            .RegisterModule(new DataDogModule(configuration))
            .RegisterModule<EnvelopeModule>()
            .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings))
            .RegisterModule<StreamStoreModule>()
            .RegisterModule<RoadNetworkSnapshotModule>()
            .RegisterModule<CommandHandlingModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<SyndicationModule>()
            ;

        var eventSourcedEntityMap = new EventSourcedEntityMap();

        services
            .AddSingleton<IStreetNameCache, StreetNameCache>()
            .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => eventSourcedEntityMap)
            .AddSingleton<IRoadNetworkCommandQueue>(sp => new RoadNetworkCommandQueue(sp.GetRequiredService<IStreamStore>(), CommandMetadata))
            .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                new CommandHandlerModule[]
                {
                    CommandModules.RoadNetwork(sp)
                }), CommandMetadata))
            ;

        builder.RegisterIdempotentCommandHandler();

        builder.Populate(services);

        return new AutofacServiceProvider(builder.Build());
    }
}
