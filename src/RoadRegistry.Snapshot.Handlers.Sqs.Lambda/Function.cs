[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.BackOffice;

public class Function : RoadRegistryLambdaFunction
{
    public Function() : base("RoadRegistry.Snapshot.Handlers.Sqs.Lambda", new[] { typeof(Sqs.DomainAssemblyMarker).Assembly })
    {
    }

    protected override void ConfigureContainer(ContainerBuilder builder, IConfiguration configuration)
    {
        base.ConfigureContainer(builder, configuration);

        builder
            .RegisterModule(new EventHandlingModule(typeof(Sqs.DomainAssemblyMarker).Assembly, EventSerializerSettings))
            .RegisterModule<ContextModule>()
            .RegisterModule<MediatorModule>()
            ;
    }

    protected override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services
            .AddRoadNetworkSnapshotStrategyOptions()
            ;

        return base.ConfigureServices(services);
    }
}
