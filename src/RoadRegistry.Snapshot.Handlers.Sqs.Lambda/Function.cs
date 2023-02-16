[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using BackOffice.Core;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Configuration;
using Hosts;
using Hosts.Infrastructure.Extensions;
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
            .RegisterAssemblyTypes(typeof(MessageHandler).Assembly)
            .AsImplementedInterfaces();

        builder
            .RegisterModule(new EventHandlingModule(typeof(Sqs.DomainAssemblyMarker).Assembly, EventSerializerSettings))
            .RegisterModule<ContextModule>()
            ;
    }

    protected override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services
            .AddRoadRegistrySnapshot()
            .AddSqsLambdaHandlerOptions()
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
