namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using System.Reflection;
using Autofac;
using Infrastructure;
using MediatorModule = BackOffice.MediatorModule;

public class Startup : TestStartup
{
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

        builder.RegisterIdempotentCommandHandler();
    }
}
