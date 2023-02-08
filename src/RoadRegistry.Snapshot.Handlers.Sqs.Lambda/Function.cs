[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Hosts;
using Microsoft.Extensions.Configuration;
using RoadRegistry.BackOffice;
using RoadRegistry.Hosts.Infrastructure.Modules;
using System.Reflection;

public class Function : RoadRegistryLambdaFunction
{
    public Function() : base("RoadRegistry.Snapshot.Handlers.Sqs.Lambda", new[] { typeof(Sqs.DomainAssemblyMarker).Assembly })
    {
    }

    protected override void ConfigureContainer(ContainerBuilder builder, IConfiguration configuration)
    {
        base.ConfigureContainer(builder, configuration);

        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder
            .RegisterModule(new EventHandlingModule(typeof(Sqs.DomainAssemblyMarker).Assembly, EventSerializerSettings))
            .RegisterModule<RoadNetworkSnapshotModule>()
            .RegisterModule<ContextModule>()
            ;
    }
}
