namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Autofac;
using MediatR;
using MediatR.Pipeline;
using System;
using System.Collections.Generic;
using System.Reflection;
using Module = Autofac.Module;

public class MediatorModule : Module
{
    private readonly IEnumerable<Type> _mediatorOpenTypes = new[]
    {
        typeof(IRequestHandler<,>),
        typeof(IRequestExceptionHandler<,,>),
        typeof(IRequestExceptionAction<,>),
        typeof(INotificationHandler<>),
        typeof(IStreamRequestHandler<,>)
    };

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();
        builder.RegisterGeneric(typeof(ValidationPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.Register<ServiceFactory>(ctx =>
        {
            var c = ctx.Resolve<IComponentContext>();
            return t => c.Resolve(t);
        });

        foreach (var mediatrOpenType in _mediatorOpenTypes)
            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .AsClosedTypesOf(mediatrOpenType)
                .AsImplementedInterfaces();

    }
}
