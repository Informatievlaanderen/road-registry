namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using Autofac;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Editor.Schema;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Product.Schema;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using SqlStreamStore;
using StreetName;
using System.Reflection;
using MediatorModule = Sqs.MediatorModule;

public class Startup : TestStartup
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder
            .RegisterModule<BackOffice.MediatorModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<BackOffice.Handlers.MediatorModule>()
            .RegisterModule<MediatorModule>()
            ;

        builder.Register<SqsLambdaHandlerOptions>(c => new FakeSqsLambdaHandlerOptions());
        builder.Register<IRoadNetworkCommandQueue>(c => new RoadNetworkCommandQueue(c.Resolve<IStreamStore>(), new ApplicationMetadata(RoadRegistryApplication.Lambda)));
        builder.Register<IRoadNetworkEventWriter>(c => new RoadNetworkEventWriter(c.Resolve<IStreamStore>(), EnrichEvent.WithTime(c.Resolve<IClock>())));
        builder.Register<IIdempotentCommandHandler>(c => new RoadRegistryIdempotentCommandHandler(c.Resolve<CommandHandlerDispatcher>()));
        builder.Register(c => Dispatch.Using(Resolve.WhenEqualToMessage(
        [
            new RoadNetworkCommandModule(
                c.Resolve<IStreamStore>(),
                c.Resolve<ILifetimeScope>(),
                new FakeRoadNetworkSnapshotReader(),
                c.Resolve<IClock>(),
                new FakeExtractUploadFailedEmailClient(),
                c.Resolve<ILoggerFactory>()
            )
        ]), ApplicationMetadata));
        builder.RegisterIdempotentCommandHandler();
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
            .AddSingleton<EventSourcedEntityMap>(_ => new EventSourcedEntityMap())
            .AddTransient<ICustomRetryPolicy>(sp => new FakeRetryPolicy())
            .AddTransient<IdempotencyContext>(sp => new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>()))
            .AddInMemoryDbContext<EditorContext>()
            .AddInMemoryDbContext<ProductContext>()
            .AddStreetNameCache();
    }
}
