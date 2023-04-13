namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using Autofac;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Product.Schema;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using SqlStreamStore;
using System.Reflection;
using MediatorModule = BackOffice.MediatorModule;

public class Startup : TestStartup
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder
            .RegisterModule<MediatorModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<BackOffice.Handlers.MediatorModule>()
            .RegisterModule<Sqs.MediatorModule>()
            .RegisterModule<Sqs.Lambda.Infrastructure.Modules.SyndicationModule>()
            ;

        builder.Register<SqsLambdaHandlerOptions>(c => new FakeSqsLambdaHandlerOptions());
        builder.Register<IRoadNetworkCommandQueue>(c => new RoadNetworkCommandQueue(c.Resolve<IStreamStore>(), new ApplicationMetadata(RoadRegistryApplication.Lambda)));
        builder.Register<IIdempotentCommandHandler>(c => new RoadRegistryIdempotentCommandHandler(c.Resolve<CommandHandlerDispatcher>()));
        builder.Register(c => Dispatch.Using(Resolve.WhenEqualToMessage(
            new CommandHandlerModule[]
            {
                new RoadNetworkCommandModule(
                    c.Resolve<IStreamStore>(),
                    c.Resolve<ILifetimeScope>(),
                    new FakeRoadNetworkSnapshotReader(),
                    c.Resolve<IClock>(),
                    c.Resolve<ILoggerFactory>()
                )
            }), ApplicationMetadata));
        builder.RegisterIdempotentCommandHandler();
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
            .AddSingleton<EventSourcedEntityMap>(_ => new EventSourcedEntityMap())
            .AddTransient<ICustomRetryPolicy>(sp => new FakeRetryPolicy())
            .AddTransient<IdempotencyContext>(sp => new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>()))
            .AddDbContext<EditorContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N")))
            .AddDbContext<ProductContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
    }
}