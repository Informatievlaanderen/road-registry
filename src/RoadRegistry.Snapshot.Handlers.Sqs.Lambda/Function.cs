using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using BackOffice;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Hosts;
using Microsoft.Extensions.Hosting;
using DomainAssemblyMarker = DomainAssemblyMarker;

public class Function : RoadRegistryLambdaFunction<MessageHandler>
{
    protected override string ApplicationName => "RoadRegistry.Snapshot.Handlers.Sqs.Lambda";

    public Function() : base(new[] { typeof(DomainAssemblyMarker).Assembly })
    {
    }

    protected override void ConfigureContainer(HostBuilderContext context, ContainerBuilder builder)
    {
        builder
            .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, EventSerializerSettings))
            .RegisterModule<ContextModule>()
            ;
    }
}
