[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Abstractions;
using Autofac;
using BackOffice.Extensions;
using BackOffice.Infrastructure.Modules;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
using Framework;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
using FluentValidation;

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
            .AddRoadNetworkEventWriter()
            .AddChangeRoadNetworkDispatcher()
            .AddCommandHandlerDispatcher(sp => Resolve.WhenEqualToMessage(
                new CommandHandlerModule[]
                {
                    CommandModules.RoadNetwork(sp)
                })
            )
            .AddValidatorsFromAssemblyContaining<BackOffice.DomainAssemblyMarker>()
            .AddLogging(configure =>
            {
                configure.AddRoadRegistryLambdaLogger();
            })
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
            .RegisterModule(new EventHandlingModule(typeof(BackOffice.Handlers.DomainAssemblyMarker).Assembly, EventSerializerSettings))
            .RegisterModule<CommandHandlingModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<SyndicationModule>()
            .RegisterModule<SqsHandlersModule>()
            ;

        builder.RegisterIdempotentCommandHandler();
    }
}
