[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Configuration;
using Hosts;
using Microsoft.Extensions.Configuration;
using RoadRegistry.BackOffice;
using RoadRegistry.Hosts.Infrastructure.Modules;
using Hosts.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

public class Function : RoadRegistryLambdaFunction
{
    public Function() : base("RoadRegistry.Snapshot.Handlers.Sqs.Lambda", new[] { typeof(Sqs.DomainAssemblyMarker).Assembly })
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
            .RegisterModule<ContextModule>()
            ;
    }

    protected override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services
            .AddTicketing()
            .AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                var options = new RoadNetworkSnapshotStrategyOptions();
                configuration.GetSection(RoadNetworkSnapshotStrategyOptions.ConfigurationSection).Bind(options);

                return options;
            })
            ;

        return base.ConfigureServices(services);
    }
}
