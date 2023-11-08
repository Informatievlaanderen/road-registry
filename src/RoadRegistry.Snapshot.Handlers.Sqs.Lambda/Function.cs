[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using BackOffice;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Hosts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Function : RoadRegistryLambdaFunction<MessageHandler>
{
    protected override string ApplicationName => "RoadRegistry.Snapshot.Handlers.Sqs.Lambda";

    public Function() : base(new[] { typeof(Sqs.DomainAssemblyMarker).Assembly })
    {
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services
            .AddRoadNetworkSnapshotStrategyOptions()
            ;
    }
    
    protected override void ConfigureContainer(HostBuilderContext context, ContainerBuilder builder)
    {
        builder
            .RegisterModule(new EventHandlingModule(typeof(Sqs.DomainAssemblyMarker).Assembly, EventSerializerSettings))
            .RegisterModule<ContextModule>()
            .RegisterModule<MediatorModule>()
            ;
    }
}
