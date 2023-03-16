[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Configuration;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.BackOffice;
using System.Configuration;
using Amazon.Lambda.SQSEvents;

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
            .RegisterOptions<RoadNetworkSnapshotStrategyOptions>(options =>
            {
                if (options.EventCount <= 0)
                {
                    throw new ConfigurationErrorsException($"{nameof(options.EventCount)} must be greater than zero");
                }
            })
            ;

        return base.ConfigureServices(services);
    }

    public async Task Handler2(SQSEvent sqsEvent, ILambdaContext context)
    {
        await base.Handler(sqsEvent, context);
    }
}
