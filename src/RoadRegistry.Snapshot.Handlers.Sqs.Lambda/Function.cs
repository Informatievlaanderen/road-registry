[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Hosts;
using Hosts.Infrastructure.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoadRegistry.BackOffice;

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

    protected override void ConfigureHealthChecks(HealthCheckInitializer builder) => builder
        .AddSqlServer()
        .AddS3(x => x
            .CheckPermission(WellknownBuckets.SnapshotsBucket, Permission.Read, Permission.Write)
            .CheckPermission(WellknownBuckets.SqsMessagesBucket, Permission.Read)
        )
        .AddTicketing()
    ;

    protected override void ConfigureContainer(HostBuilderContext context, ContainerBuilder builder)
    {
        builder
            .RegisterModule(new EventHandlingModule(typeof(Sqs.DomainAssemblyMarker).Assembly, EventSerializerSettings))
            .RegisterModule<ContextModule>()
            .RegisterModule<MediatorModule>()
            ;
    }
}
