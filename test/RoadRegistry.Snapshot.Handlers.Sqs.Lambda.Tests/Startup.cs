namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests;
using Autofac;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Hosts.Infrastructure.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using SqlStreamStore;
using System.Reflection;
using MediatorModule = Sqs.MediatorModule;

public class Startup : TestStartup
{
    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder.Register<SqsLambdaHandlerOptions>(c => new FakeSqsLambdaHandlerOptions());
        builder.Register<IRoadNetworkCommandQueue>(c => new RoadNetworkCommandQueue(c.Resolve<IStreamStore>(), new ApplicationMetadata(RoadRegistryApplication.Lambda)));

        builder
            .RegisterModule<BackOffice.MediatorModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<BackOffice.Handlers.MediatorModule>()
            .RegisterModule<MediatorModule>()
            .RegisterModule<BlobClientModule>()
            ;
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        var configuration = hostBuilderContext.Configuration;

        services
            .AddSingleton<EventSourcedEntityMap>(_ => new EventSourcedEntityMap())
            .AddTransient<ICustomRetryPolicy>(sp => new FakeRetryPolicy())
            .AddRoadRegistrySnapshot()
            .AddSingleton<IRoadNetworkSnapshotReader, FakeRoadNetworkSnapshotReader>()
            .AddSingleton<IRoadNetworkSnapshotWriter, FakeRoadNetworkSnapshotWriter>()
            ;
    }
}
