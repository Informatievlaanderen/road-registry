namespace RoadRegistry.BackOffice.MessagingHost.Sqs;

using Autofac;
using MediatR;
using MediatR.Pipeline;
using System;
using System.Collections.Generic;

public class MediatorModule : Autofac.Module
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
