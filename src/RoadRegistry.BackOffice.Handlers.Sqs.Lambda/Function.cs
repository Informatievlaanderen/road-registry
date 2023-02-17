[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Abstractions;
using Autofac;
using BackOffice.Extensions;
using BackOffice.Infrastructure.Modules;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
using Core;
using Framework;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.Modules;
using Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public sealed class Function : RoadRegistryLambdaFunction
{
    public Function()
        : base("RoadRegistry.BackOffice.Handlers.Sqs.Lambda",
            new List<Assembly>
            {
                typeof(BackOffice.Handlers.Sqs.DomainAssemblyMarker).Assembly,
                typeof(RoadRegistry.BackOffice.Abstractions.BlobRequest).Assembly
            })
    {
    }
    
    protected override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton<IStreetNameCache, StreetNameCache>()
            .AddDistributedStreamStoreLockOptions()
            .AddRoadNetworkCommandQueue()
            .AddCommandHandlerDispatcher(sp => Resolve.WhenEqualToMessage(
                new CommandHandlerModule[]
                {
                    CommandModules.RoadNetwork(sp)
                }))
                .AddLogging(configure =>
                {
                    configure.AddRoadRegistryLambdaLogger();
                })
            .AddRoadRegistrySnapshot()
            ;

        return base.ConfigureServices(services);
    }

    protected override void ConfigureContainer(ContainerBuilder builder, IConfiguration configuration)
    {
        base.ConfigureContainer(builder, configuration);

        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder
            .RegisterModule(new DataDogModule(configuration))
            .RegisterModule<EnvelopeModule>()
            .RegisterModule(new EventHandlingModule(typeof(BackOffice.Handlers.DomainAssemblyMarker).Assembly, eventSerializerSettings))
            .RegisterModule<CommandHandlingModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<SyndicationModule>()
            .RegisterModule<SqsHandlersModule>()
            .RegisterModule<BlobClientModule>()
            ;

        builder.RegisterIdempotentCommandHandler();
    }
}
