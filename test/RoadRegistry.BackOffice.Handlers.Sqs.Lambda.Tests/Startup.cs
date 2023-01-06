namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using Autofac;
using Hosts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoadRegistry.BackOffice.Framework;
using MediatorModule = BackOffice.MediatorModule;

public class Startup : TestStartup
{
    protected override CommandHandlerDispatcher ConfigureCommandHandlerDispatcher(IServiceProvider sp)
    {
        return Dispatch.Using(Resolve.WhenEqualToMessage(
            new CommandHandlerModule[]
            {
                CommandModules.RoadNetwork(sp)
            }));
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule<MediatorModule>();
        builder.RegisterModule<ContextModule>();
        builder.RegisterModule<BackOffice.Handlers.MediatorModule>();
        builder.RegisterModule<Sqs.MediatorModule>();
        builder.RegisterModule<Sqs.Lambda.Infrastructure.Modules.SyndicationModule>();
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services.AddSingleton<Func<EventSourcedEntityMap>>(_ => () => new EventSourcedEntityMap());

        //services
        //    .AddDbContext<EditorContext>((sp, options) => options
        //        .UseLoggerFactory(sp.GetService<ILoggerFactory>())
        //        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        //        .UseInMemoryDatabase(Guid.NewGuid().ToString("N")))
        //    .AddDbContext<ProductContext>((sp, options) => options
        //        .UseLoggerFactory(sp.GetService<ILoggerFactory>())
        //        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        //        .UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
    }
}
