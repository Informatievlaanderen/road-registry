namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using System.Reflection;
using Autofac;
using Infrastructure.Extensions;
using Infrastructure.Modules;
using MediatorModule = BackOffice.Handlers.MediatorModule;

public class Startup : TestStartup
{
    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder
            .RegisterModule<BackOffice.MediatorModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<MediatorModule>()
            .RegisterModule<Sqs.MediatorModule>()
            .RegisterModule<SyndicationModule>()
            ;

        builder.RegisterIdempotentCommandHandler();
    }
}
