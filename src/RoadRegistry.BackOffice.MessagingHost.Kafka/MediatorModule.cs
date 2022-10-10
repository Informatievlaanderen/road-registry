namespace RoadRegistry.BackOffice.MessagingHost.Kafka;

using Autofac;
using MediatR;
using System;
using System.Collections.Generic;

public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMediator).Assembly).AsImplementedInterfaces();
        builder.RegisterGeneric(typeof(ValidationPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        builder.Register<ServiceFactory>(ctx =>
        {
            var c = ctx.Resolve<IComponentContext>();
            return t => c.Resolve(t);
        });

        builder.RegisterModule(new Handlers.MediatorModule());
    }
}
