namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests;

using Autofac;
using BackOffice;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoadRegistry.BackOffice.Framework;
using System.Reflection;
using RoadRegistry.Tests.Framework;
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
            ;
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        var eventSourcedEntityMap = new EventSourcedEntityMap();

        services
            .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => eventSourcedEntityMap)
            .AddTransient<ICustomRetryPolicy>(sp => new FakeRetryPolicy())
            ;
    }
}
