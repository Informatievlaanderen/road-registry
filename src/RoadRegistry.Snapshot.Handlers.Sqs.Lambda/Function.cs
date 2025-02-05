using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using BackOffice;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Hosts;
using Microsoft.Extensions.Hosting;

public class Function : RoadRegistryLambdaFunction<MessageHandler>
{
    protected override string ApplicationName => "RoadRegistry.Snapshot.Handlers.Sqs.Lambda";

    public Function() : base([typeof(SnapshotHandlersSqsAssemblyMarker).Assembly])
    {
    }

    protected override void ConfigureContainer(HostBuilderContext context, ContainerBuilder builder)
    {
        builder
            .RegisterModule(new EventHandlingModule(typeof(SnapshotHandlersSqsAssemblyMarker).Assembly, EventSerializerSettings))
            .RegisterModule<ContextModule>();
    }
}
