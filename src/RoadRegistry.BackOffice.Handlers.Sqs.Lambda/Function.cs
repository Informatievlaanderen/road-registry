using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using System.Reflection;
using Abstractions;
using Autofac;
using BackOffice.Extensions;
using BackOffice.Infrastructure.Modules;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Framework;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.Modules;
using Infrastructure.Extensions;
using Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DomainAssemblyMarker = BackOffice.Handlers.DomainAssemblyMarker;

public sealed class Function : RoadRegistryLambdaFunction
{
    public Function() : base("RoadRegistry.BackOffice.Handlers.Sqs.Lambda", new[] { typeof(DomainAssemblyMarker).Assembly })
    {
    }

    protected override void ConfigureContainer(ContainerBuilder builder, IConfiguration configuration)
    {
        base.ConfigureContainer(builder, configuration);

        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder
            .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, EventSerializerSettings))
            .RegisterModule<RoadNetworkSnapshotModule>()
            .RegisterModule<CommandHandlingModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<SyndicationModule>()
            ;

        builder.RegisterIdempotentCommandHandler();
    }

    protected override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services
            .AddTicketing()
            .AddSingleton<IStreetNameCache, StreetNameCache>()
            .AddRoadNetworkCommandQueue()
            .AddEditorContext()
            .AddCommandHandlerDispatcher(sp => Resolve.WhenEqualToMessage(
                new CommandHandlerModule[]
                {
                    CommandModules.RoadNetwork(sp)
                }))
            ;

        return base.ConfigureServices(services);
    }
}
