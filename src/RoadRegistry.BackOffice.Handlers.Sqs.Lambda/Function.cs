[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Abstractions;
using Autofac;
using BackOffice.Extensions;
using BackOffice.Infrastructure.Modules;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Framework;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.Modules;
using Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class Function : RoadRegistryLambdaFunction
{
    public Function() : base("RoadRegistry.BackOffice.Handlers.Sqs.Lambda", new[] { typeof(Sqs.DomainAssemblyMarker).Assembly })
    {
    }

    protected override void ConfigureContainer(ContainerBuilder builder, IConfiguration configuration)
    {
        base.ConfigureContainer(builder, configuration);

        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).Assembly)
            .AsImplementedInterfaces();

        builder
            .RegisterModule(new EventHandlingModule(typeof(Sqs.DomainAssemblyMarker).Assembly, EventSerializerSettings))
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
            .AddSingleton<IStreetNameCache, StreetNameCache>()
            .AddRoadNetworkCommandQueue()
            .AddSqsLambdaHandlerOptions()
            .AddCommandHandlerDispatcher(sp => Resolve.WhenEqualToMessage(
                new CommandHandlerModule[]
                {
                    CommandModules.RoadNetwork(sp)
                }))
            ;

        return base.ConfigureServices(services);
    }
}
